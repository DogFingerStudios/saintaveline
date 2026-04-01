using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.TerrainTools;
#else
using UnityEngine.Experimental.TerrainAPI;
#endif

namespace sc.terrain.proceduralpainter
{
    public class HeatmapPreview
    {
        private static Material utilsMat;
        private static Shader utilsShader;

#if UNITY_2019_3_OR_NEWER
        private static float kNormalizedHeightScale => PaintContext.kNormalizedHeightScale;
#else
        private const float kNormalizedHeightScale = 0.4999771f;
#endif
        
        public static void CreateHeatmaps(Terrain[] terrains, int layerIndex, ref RenderTexture[] heatmaps)
        {
            heatmaps = new RenderTexture[terrains.Length];

            for (int i = 0; i < heatmaps.Length; i++)
            {
                heatmaps[i] = CreateHeatmap(terrains[i], layerIndex);
            }
        }

        private static RenderTexture CreateHeatmap(Terrain terrain, int layerIndex)
        {
            RenderTexture rt = new RenderTexture(terrain.terrainData.alphamapResolution, terrain.terrainData.alphamapResolution, 0, GraphicsFormat.R8_UNorm);

            var splatIndex = Utilities.GetSplatmapIndex(layerIndex);
            var channelIndex = Utilities.GetChannelIndex(layerIndex);

            Texture2D splatmap = terrain.terrainData.GetAlphamapTexture(splatIndex);
            
            if (utilsShader == null) utilsShader = Shader.Find("Hidden/TerrainEngine/TerrainLayerUtils");
            if (utilsMat == null) utilsMat = new Material(utilsShader);
            
            utilsMat.SetTexture("_MainTex", splatmap);
            utilsMat.SetVector("_LayerMask", Utilities.GetVectorMask(channelIndex));
            
            //Pass 0 = Select one channel and copy it into R channel
            Graphics.Blit(splatmap, rt, utilsMat, 0);

            return rt;
        }
        
        private static Material heatmapMat;
        
        public static void Draw(Terrain terrain, TerrainLayer layer, Texture alphaMap, bool contour, bool tilingPreview)
        {
            if (!terrain || !layer) return;
            
            if (!heatmapMat) heatmapMat = new Material(Shader.Find("Hidden/TerrainPainter/Heatmap"));
            
            Texture heightmapTexture = terrain.terrainData.heightmapTexture;

            RectInt pixelRect = new RectInt(0, 0, heightmapTexture.width, heightmapTexture.height);
            Vector2 pixelSize = new Vector2(terrain.terrainData.size.x / heightmapTexture.width, terrain.terrainData.size.z / heightmapTexture.height);
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, new Vector2(0.5f, 0.5f), terrain.terrainData.size.x, 0.0f);
            
            // we want to build a quad mesh, with one vertex for each pixel in the heightmap
            // i.e. a 3x3 heightmap would create a mesh that looks like this:
            //
            //    +-+-+
            //    |\|\|
            //    +-+-+
            //    |\|\|
            //    +-+-+
            //
            int quadsX = pixelRect.width+1;
            int quadsY = pixelRect.height+1;
            int vertexCount = quadsX * quadsY * (2 * 3);  // two triangles (2 * 3 vertices) per quad

            // issue: the 'int vertexID' in the shader is often stored in an fp32
            // which can only represent exact integers up to 16777216 ~== 6 * 1672^2
            // once we have more than 16777216 vertices, the vertexIDs start skipping odd values, resulting in missing triangles
            // the solution is to reduce vertex count by halving our mesh resolution before we hit that point
            const int kMaxFP32Int = 16777216;
            int vertSkip = 1;
            while (vertexCount > kMaxFP32Int / 2)   // in practice we want to stay well below 16 million verts, for perf sanity
            {
                quadsX = (quadsX + 1) / 2;
                quadsY = (quadsY + 1) / 2;
                vertexCount = quadsX * quadsY * (2 * 3);
                vertSkip *= 2;
            }

            // this is used to tessellate the quad mesh (from within the vertex shader)
            heatmapMat.SetVector("_QuadRez", new Vector4(quadsX, quadsY, vertexCount, vertSkip));

            // paint context pixels to heightmap uv:   uv = (pixels + 0.5) / width
            float invWidth = 1.0f / heightmapTexture.width;
            float invHeight = 1.0f / heightmapTexture.height;
            heatmapMat.SetVector("_HeightmapUV_PCPixelsX",  new Vector4(invWidth, 0.0f, 0.0f, 0.0f));
            heatmapMat.SetVector("_HeightmapUV_PCPixelsY",  new Vector4(0.0f, invHeight, 0.0f, 0.0f));
            heatmapMat.SetVector("_HeightmapUV_Offset",     new Vector4(0.5f * invWidth, 0.5f * invHeight, 0.0f, 0.0f));
            
            heatmapMat.SetTexture("_Heightmap", heightmapTexture);
            heatmapMat.SetTexture("_NormalMap", terrain.normalmapTexture);

            // paint context pixels to object (terrain) position
            // objectPos.x = scaleX * pcPixels.x + heightmapRect.xMin * scaleX
            // objectPos.y = scaleY * H
            // objectPos.z = scaleZ * pcPixels.y + heightmapRect.yMin * scaleZ
            float scaleX = pixelSize.x;
            float scaleY = (terrain.terrainData.heightmapScale.y) / kNormalizedHeightScale;
            float scaleZ = pixelSize.y;
            heatmapMat.SetVector("_ObjectPos_PCPixelsX", new Vector4(scaleX, 0.0f, 0.0f, 0.0f));
            heatmapMat.SetVector("_ObjectPos_HeightMapSample", new Vector4(0.0f, scaleY, 0.0f, 0.0f));
            heatmapMat.SetVector("_ObjectPos_PCPixelsY", new Vector4(0.0f, 0.0f, scaleZ, 0.0f));
            //Note slightly offset, so raise up so it doesn't clip through terrain
            heatmapMat.SetVector("_ObjectPos_Offset", new Vector4((pixelRect.xMin * scaleX), 3f , (pixelRect.yMin * scaleZ) + (pixelSize.y * 0.0f) , 1.0f));

            // paint context origin in terrain space
            // (note this is the UV space origin and size, not the mesh origin & size)
            float pcOriginX = pixelRect.xMin * pixelSize.x;
            float pcOriginZ = pixelRect.yMin * pixelSize.y;
            float pcSizeX = pixelSize.x;
            float pcSizeZ = pixelSize.y;

            Vector2 scaleU = pcSizeX * brushXform.targetX;
            Vector2 scaleV = pcSizeZ * brushXform.targetY;
            Vector2 offset = brushXform.targetOrigin + pcOriginX * brushXform.targetX + pcOriginZ * brushXform.targetY;
            heatmapMat.SetVector("_BrushUV_PCPixelsX",      new Vector4(scaleU.x, scaleU.y, 0.0f, 0.0f));
            heatmapMat.SetVector("_BrushUV_PCPixelsY",      new Vector4(scaleV.x, scaleV.y, 0.0f, 0.0f));
            heatmapMat.SetVector("_BrushUV_Offset",         new Vector4(offset.x, offset.y, 0.0f, 1.0f));
            heatmapMat.SetTexture("_LayerMaskTex", alphaMap);
            
            heatmapMat.SetFloat("_ContourOn", contour ? 1 : 0);
            heatmapMat.SetFloat("_TilingOn", tilingPreview ? 1 : 0);
            //Tiling in world-space
            heatmapMat.SetVector("_LayerTiling", new Vector4(layer.diffuseTexture.width / terrain.terrainData.size.x / layer.tileSize.x, layer.diffuseTexture.height / terrain.terrainData.size.z / layer.tileSize.y,  0, 0));
            
            heatmapMat.SetVector("_TerrainObjectToWorldOffset", terrain.GetPosition());

            heatmapMat.SetPass(0);
#if UNITY_2019_1_OR_NEWER
            Graphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
#else
            Graphics.DrawProcedural(MeshTopology.Triangles, vertexCount);
#endif
        }
    }
}