using UnityEngine;
namespace RTS_Demo
{
    [System.Serializable]
    public class Timer
    {
        [SerializeField] private float _duration;
        private float _endTime;
        private bool _isRunning;

        public bool IsRunning { get => _isRunning; }
        public bool IsCompleted { get => _isRunning && Time.time >= _endTime; }
        public float Progress { get => !_isRunning ? 0f : Mathf.Clamp01((_duration - (_endTime - Time.time)) / _duration); }

        public void Start()
        {
            _isRunning = true;
            _endTime = Time.time + _duration;
        }
        public void Stop()
        {
            _isRunning = false;
        }
    }
}
