using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExtendVariable<T>
{    
    public abstract T getValue();
    public abstract void setValue(T value);
    public abstract void loadFromXML(string valueString);
}

public class Vector3Ex : ExtendVariable<Vector3>
{
    private FloatEx _x = new FloatEx();
    private FloatEx _y = new FloatEx();
    private FloatEx _z = new FloatEx();

    public float x{get{return _x.getValue();}}
    public float y{get{return _y.getValue();}}
    public float z{get{return _z.getValue();}}

    public override Vector3 getValue()
    {
        return new Vector3(_x,_y,_z);
    }

    public override void setValue(Vector3 value)
    {
        _x.setValue(value.x);
        _y.setValue(value.y);
        _z.setValue(value.z);
    }

    public override void loadFromXML(string valueString)
    {
        string[] splitted = valueString.Split(' ');
        if(splitted.Length != 3 && splitted.Length != 2)
        {
            DebugUtil.assert(false,"invalid vector3 string: {0}", valueString);
            return;
        }

        _x.loadFromXML(splitted[0]);
        _y.loadFromXML(splitted[1]);

        if(splitted.Length == 3)
            _z.loadFromXML(splitted[2]);
    }
}

public class Vector2Ex : ExtendVariable<Vector2>
{
    private FloatEx _x = new FloatEx();
    private FloatEx _y = new FloatEx();

    public float x{get{return _x.getValue();}}
    public float y{get{return _y.getValue();}}

    public override Vector2 getValue()
    {
        return new Vector2(_x,_y);
    }

    public override void setValue(Vector2 value)
    {
        _x.setValue(value.x);
        _y.setValue(value.y);
    }

    public override void loadFromXML(string valueString)
    {
        string[] splitted = valueString.Split(' ');
        if(splitted.Length != 2)
        {
            DebugUtil.assert(false,"invalid vector2 string: {0}", valueString);
            return;
        }

        _x.loadFromXML(splitted[0]);
        _y.loadFromXML(splitted[1]);
    }
}

public class FloatEx : ExtendVariable<float>
{
    private float _value = 0f;

    private float _randomMin = 0f;
    private float _randomMax = 0f;

    private bool _isRandom = false;

    public static implicit operator float(FloatEx floatEx) => floatEx.getValue();

    public static float operator +(FloatEx a) => a.getValue();
    public static float operator -(FloatEx a) => -a.getValue();

    public static float operator +(FloatEx a, FloatEx b) => a.getValue() + b.getValue();
    public static float operator +(float a, FloatEx b) => a + b.getValue();
    public static float operator +(FloatEx a, float b) => a.getValue() + b;

    public static float operator -(FloatEx a, FloatEx b) => a.getValue() - b.getValue();
    public static float operator -(FloatEx a, float b) => a.getValue() - b;
    public static float operator -(float a, FloatEx b) => a - b.getValue();

    public static float operator *(FloatEx a, FloatEx b) => a.getValue() * b.getValue();
    public static float operator *(FloatEx a, float b) => a.getValue() * b;
    public static float operator *(float a, FloatEx b) => a * b.getValue();

    public static float operator /(FloatEx a, FloatEx b) => a.getValue() * (1f / b.getValue());
    public static float operator /(FloatEx a, float b) => a.getValue() * (1f / b);
    public static float operator /(float a, FloatEx b) => a * (1f / b.getValue());

    public override string ToString() => $"{getValue()}";

    public override float getValue()
    {
        if(_isRandom)
            return Random.Range(_randomMin, _randomMax);
        
        return _value;
    }

    public override void setValue(float value)
    {
        _value = value;
    }

    public override void loadFromXML(string valueString)
    {
        if(float.TryParse(valueString,out float result))
        {
            _value = result;
            _isRandom = false;
            return;
        }

        if(valueString.Contains("Random_") == false)
        {
            DebugUtil.assert(false,"잘못된 Float Type Value : {0}", valueString);
            return;
        }

        string[] randomRangeString = valueString.Replace("Random_",string.Empty).Split('^');
        if(randomRangeString.Length != 2)
        {
            DebugUtil.assert(false,"잘못된 Float Type Value : {0}", valueString);
            return;
        }

        float min, max;
        if(float.TryParse(randomRangeString[0],out min) == false)
        {
            DebugUtil.assert(false,"잘못된 Float Type Value : {0}", valueString);
            return;
        }

        if(float.TryParse(randomRangeString[1],out max) == false)
        {
            DebugUtil.assert(false,"잘못된 Float Type Value : {0}", valueString);
            return;
        }

        _randomMax = max;
        _randomMin = min;
        _isRandom = true;
    }

    
}