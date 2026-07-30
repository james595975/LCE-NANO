using System;
using System.Collections;
using UnityEngine;

namespace LCENano
{
    public class PlayerDiagnostics : MonoBehaviour
    {
        public MicroSwimmer swimmer;
        public IEnumerator Start()
        {
            swimmer.parameters.centerlineSpeedMmS = 0f;
            swimmer.parameters.stroke = StrokeMode.TravelingWave;
            swimmer.parameters.motionVisualizationGain = 100f;
            swimmer.ResetPosition();
            Vector3 initial = swimmer.transform.position;
            yield return new WaitForSeconds(5f);
            Debug.Log($"LCE_DIAGNOSTIC initial={initial} final={swimmer.transform.position} " +
                      $"delta={swimmer.transform.position - initial} speed_um_s={swimmer.RFT.speedMS * 1e6f}");
            Application.Quit();
        }
    }
}
