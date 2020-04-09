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
                b.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        public void Focus(Bone bone)
        {
            foreach (Bone b in bones)
                b.gameObject.layer = LayerMask.NameToLayer("Fade Out");
            if (bone != null)
                bone.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        public abstract void ResetPositions();

        public IEnumerator<Bone> GetEnumerator()
        {
            return ((IEnumerable<Bone>)bones).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Bone>)bones).GetEnumerator();
        }
    }
}
