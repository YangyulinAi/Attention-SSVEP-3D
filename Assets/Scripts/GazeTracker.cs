using Tobii.G2OM;
using UnityEngine;
using UnityEngine.UI; // Include UI namespace to work with Images

namespace Tobii.XR.Examples.GettingStarted
{
    public class GazeTracker : MonoBehaviour, IGazeFocusable
    {
        public Image childImage;


        public void GazeFocusChanged(bool hasFocus)
        {
            // Check if the childImage has been assigned before trying to change its active state

            Debug.Log(hasFocus);
            if (childImage != null)
            {
                // Set the child image active or inactive based on gaze focus
                childImage.gameObject.SetActive(hasFocus);
            }
        }

        private void Start()
        {
            if (childImage == null)
            {
                Debug.LogWarning("Child Image is not set on HighlightAtGaze script.");
            }
        }

       
    }
}
