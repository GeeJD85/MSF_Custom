using UnityEngine;
using UnityEngine.UI;

namespace GW.Master
{
    public class Loading_Panel : MonoBehaviour
    {
        public Image loadingCircle;

        void Update()
        {
            loadingCircle.transform.Rotate(-Vector3.forward, Time.deltaTime * 360 * 2);
        }
    }
}