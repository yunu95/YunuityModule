using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
//It acts as a MonoBehaviour wrapper object for timer module.
public class Timer : MonoBehaviour
{
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }
    [Serializable]
    public class UpdateListener
    {
        [SerializeField] private FloatEvent _listener;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private bool _inputNormalized = true;
        [SerializeField] private bool _inputModulo = false;
        [SerializeField] private bool _outputIntegral = false;
        public FloatEvent listener { get { return _listener; } }
        public AnimationCurve curve { get { return _curve; } }
        public bool inputNormalized { get { return _inputNormalized; } }
        public bool inputModulo { get { return _inputModulo; } }
        public bool outputIntegral { get { return _outputIntegral; } }
    }
    [SerializeField] private float _duration = 1;
    [SerializeField] private float _elapsed = 0;
    [SerializeField] private float _playrate = 1;
    [SerializeField] private bool _active = false;
    [SerializeField] private bool _repeat = false;
    [SerializeField] private UnityEvent _onStart;
    [SerializeField] private UnityEvent _onComplete;
    [SerializeField] private UpdateListener[] _onUpdate;
    public float duration { get { return _duration; } set { _duration = value; } }
    public float elapsed { get { return Mathf.Clamp(_elapsed, 0, _duration); } set { _elapsed = Mathf.Clamp(value, 0, _duration); } }
    public float playrate { get { return _playrate; } set { _playrate = value; } }
    public bool active { get { return _active; } set { _active = value; } }
    public float progress
    {
        get { return Mathf.Clamp(_elapsed / _duration, 0, 1); }
        set { _elapsed = Mathf.Clamp(duration * value, 0, _duration); }
    }
    private void Update()
    {
        if (!_active)
            return;
        _elapsed += Time.deltaTime * _playrate;
        foreach (var each in _onUpdate)
        {
            float input = each.inputNormalized ? progress : elapsed;
            if (each.inputModulo)
                input %= each.curve.keys[each.curve.length - 1].time;
            input = each.curve.Evaluate(input);
            if (each.outputIntegral)
            {
                input *= Time.deltaTime * _playrate;
                if (each.inputNormalized)
                    input /= _duration;
            }
            each.listener.Invoke(input);
        }
        if ((_elapsed > _duration && _playrate > 0) || (_elapsed <= 0 && _playrate < 0))
        {
            _active = false;
            _onComplete.Invoke();
            if (_repeat)
                Run();
        }
    }
    public void Run()
    {
        _active = true;
        _elapsed = (_playrate > 0) ? 0 : _duration;
        _onStart.Invoke();
    }
    public void Stop()
    {
        _active = false;
    }
    public void Continue()
    {
        _active = true;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {

        foreach (var each in _onUpdate.Select((UpdateListener element) => element.listener))
        {
            Dictionary<Interpolator, bool> found_interpolator = new Dictionary<Interpolator, bool>();
            for (int i = 0; i < each.GetPersistentEventCount(); i++)
            {
                if (each.GetPersistentTarget(i) is Interpolator && !found_interpolator.ContainsKey(each.GetPersistentTarget(i) as Interpolator))
                    found_interpolator.Add(each.GetPersistentTarget(i) as Interpolator, true);
                if (each.GetPersistentMethodName(i) == "" && each.GetPersistentTarget(i) is GameObject)
                {
                    foreach (Interpolator interpolator in (each.GetPersistentTarget(i) as GameObject).transform.GetComponents<Interpolator>())
                    {
                        if (!found_interpolator.ContainsKey(interpolator))
                        {
                            interpolator.RegisterPersistentSetter(each, i);
                            found_interpolator.Add(interpolator, true);
                        }
                    }
                }

            }
        }

    }
#endif
}
