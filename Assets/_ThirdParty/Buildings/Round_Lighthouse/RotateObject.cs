/* Author:	    Matthew Vale
 * Role:        Lead Game Developer
 * Company:		Red Phoenix Studios
*/
using UnityEngine;

namespace RPSCore {

    public class RotateObject : MonoBehaviour {

        #region VARIABLES - USE SUB-REGIONS 
        #region ROTATING OBJECTS
        public Transform objectToRotate;
        public float rotationSpeed = 2f;
        public Vector3 rotationDirection;
        public Space worldSpace;
        #endregion
        #region UPDATE METHODS
        public UpdateMethodType updateMethodType;
        public enum UpdateMethodType {
            UPDATE, FIXEDUPDATE, LATEUPDATE
        }
        #endregion
        #endregion

        #region UPDATE METHODS
        protected void LateUpdate() {
            if (updateMethodType == UpdateMethodType.LATEUPDATE) {
                objectToRotate.Rotate(-rotationSpeed * Time.deltaTime * rotationDirection, worldSpace);
            }
        }

        protected void FixedUpdate() {
            if (updateMethodType == UpdateMethodType.FIXEDUPDATE) {
                objectToRotate.Rotate(-rotationSpeed * Time.deltaTime * rotationDirection, worldSpace);
            }
        }

        protected void Update() {
            if (updateMethodType == UpdateMethodType.UPDATE) {
                objectToRotate.Rotate(-rotationSpeed * Time.deltaTime * rotationDirection, worldSpace);
            }
        }
        #endregion

    }

}