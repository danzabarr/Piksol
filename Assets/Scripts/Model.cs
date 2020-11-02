using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public abstract class Model : MonoBehaviour, IEnumerable<Bone>
    {
        protected Bone[] bones;

        public void ClearFocus()
        {
            foreach (Bone b in bones)
                b.gameObject.layer = gameObject.layer;
        }

        public void Focus(Bone bone)
        {
            foreach (Bone b in bones)
                b.gameObject.layer = LayerMask.NameToLayer("Fade Out");
            if (bone != null)
                bone.gameObject.layer = gameObject.layer;
        }

        public abstract void ResetPositions();

        public IEnumerator<Bone> GetEnumerator()
        {
            if (bones == null)
                return null;
            return ((IEnumerable<Bone>)bones).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (bones == null)
                return null;
            return ((IEnumerable<Bone>)bones).GetEnumerator();
        }
    }
}
