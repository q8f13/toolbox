using UnityEngine;

public class qfPID : MonoBehaviour
{
    public bool flagUsePI = false;
    public bool flagUsePD = false;

    public float kp = 0.5f;
    public float ki = 0.2f;
    public float kd = -0.2f;

    float _accumVal = 0.0f;
    float _lastErr = float.NaN;


    public float Tick(float curr, float target)
    {
        float err = target - curr;
        float u = kp * err;
        if(flagUsePI)
        {
            u += ki * _accumVal;
            _accumVal += err;
        }

        if(flagUsePD)
        {
            if(!float.IsNaN(_lastErr))
            {
                float d = kd * ( u - _lastErr);
                u+=d;
            }
            _lastErr = err;
        }

        return u;
    }

    public void Reset()
    {
        _lastErr = float.NaN;
        _accumVal = 0.0f;
    }
}
