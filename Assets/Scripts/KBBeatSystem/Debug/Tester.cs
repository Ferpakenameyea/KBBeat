using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KBBeat.Debugger
{
    internal class Tester : MonoBehaviour
    {
        [SerializeField] private GameObject start;
        [SerializeField] private GameObject end;
        [SerializeField] private GameObject body;
        private Vector3 scale;

        private void Start() 
        {
            scale = body.transform.localScale;
        }

        private void Update() 
        {
            var startPos = start.transform.localPosition;    
            var endPos = end.transform.localPosition;

            var newScale = scale;
            newScale.z = (endPos.z - startPos.z);

            body.transform.localPosition = (startPos + endPos) / 2;
            body.transform.localScale = newScale;
        }
    }
}
