using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTS_Demo2
{
    public class DelayedActivator : MonoBehaviour
    {
        [SerializeField] private Timer _activatorTimer;
        [SerializeField] private MonoBehaviour _componentToActivate;

        private void Start()
        {
            _activatorTimer.Start();
        }

        private void Update()
        {
            if (_activatorTimer.IsCompleted)
            {
                _componentToActivate.enabled = true;
                Destroy(this);
            }
        }
    }
}
