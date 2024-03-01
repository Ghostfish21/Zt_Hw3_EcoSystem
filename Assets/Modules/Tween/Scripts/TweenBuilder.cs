using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Tween.Scripts {
    public class TweenBuilder {
        private readonly Dictionary<string, object> properties = new();

        // 如果 该 PropertyName 已经存在，则会将旧值用新值覆盖
        public TweenBuilder addProperty(string propertyName, string propertyValue) {
            properties[propertyName] = propertyValue;
            return this;
        }
        public TweenBuilder addProperty(string propertyName, object propertyValue) {
            properties[propertyName] = propertyValue;
            return this;
        }
        
        public string getProperty(string propertyName) {
            if (properties.ContainsKey(propertyName))
                return properties[propertyName].ToString();
            return null;
        }

        public TweenBuilder setSetter(Action<float> setter) {
            return addProperty("setter", setter);
        }
        public TweenBuilder setSetterX(Action<float> setter) {
            return addProperty("setter-x", setter);
        }
        public TweenBuilder setSetterY(Action<float> setter) {
            return addProperty("setter-y", setter);
        }
        public TweenBuilder setSetterZ(Action<float> setter) {
            return addProperty("setter-z", setter);
        }

        public TweenBuilder setEase(Ease ease) {
            Func<float, float> easing = x => ease.func(x);
            return addProperty("ease", easing);
        }
        public TweenBuilder setEase(Func<float, float> ease) {
            return addProperty("ease", ease);
        }

        public TweenBuilder setEaseX(Ease ease) {
            Func<float, float> easing = x => ease.func(x);
            return addProperty("ease-x", easing);
        }
        public TweenBuilder setEaseX(Func<float, float> ease) {
            return addProperty("ease-x", ease);
        }

        public TweenBuilder setEaseY(Ease ease) {
            Func<float, float> easing = x => ease.func(x);
            return addProperty("ease-y", easing);
        }
        public TweenBuilder setEaseY(Func<float, float> ease) {
            return addProperty("ease-y", ease);
        }

        public TweenBuilder setEaseZ(Ease ease) {
            Func<float, float> easing = x => ease.func(x);
            return addProperty("ease-z", easing);
        }
        public TweenBuilder setEaseZ(Func<float, float> ease) {
            return addProperty("ease-z", ease);
        }

        public TweenBuilder setOnComplete(Action onComplete) {
            return addProperty("on-complete", onComplete);
        }
        public TweenBuilder setOnCompleteX(Action onComplete) {
            return addProperty("on-complete-x", onComplete);
        }
        public TweenBuilder setOnCompleteY(Action onComplete) {
            return addProperty("on-complete-y", onComplete);
        }
        public TweenBuilder setOnCompleteZ(Action onComplete) {
            return addProperty("on-complete-z", onComplete);
        }

        public TweenBuilder setProperties(string properties) {
            string[] propertiesArray = properties.Split(";");
            foreach (string property in propertiesArray) {
                if (property.Trim() == "") continue;
                string newProperty = property.Trim();
                string[] propertyComp = newProperty.Split(":");
                addProperty(propertyComp[0].Trim(), propertyComp[1].Trim());
            }
            return this;
        }

        private TweenManager tweenManager = null;
        public void register<T>() {
            Type type = typeof(T);
            switch(type) {
                case var t when t == typeof(float):
                    string id;
                    float startValue;
                    float endValue;
                    float duration;
                    Action onComplete;
                    Action<float> setter;
                    Func<float, float> ease;

                    id = parseId();
                
                    startValue = parseFloatValue("start-value");
                    missingParamIfNullf(startValue);

                    endValue = parseFloatValue("end-value");
                    missingParamIfNullf(endValue);

                    duration = parseFloatValue("duration");
                    missingParamIfNullf(duration);

                    ease = parseEaseValue("ease");
                    missingParamIfNull(ease);
                
                    onComplete = parseOnComplete("on-complete");

                    setter = parseSetter("setter");
                    missingParamIfNull(setter);

                    Modules.Tween.Scripts.Tween tween = new();
                    tween.setId(id).setStartValue(startValue).setEndValue(endValue)
                        .setDuration(duration).setOnComplete(onComplete).setSetter(setter)
                        .setEasing(ease);
                    GameObject.Find("Tween Manager").GetComponent<TweenManager>().register(tween);
                    break;

                case var t when t == typeof(Vector3):
                    string id1;
                    Vector3 startValue1;
                    Vector3 endValue1;
                    float durationX;
                    float durationY;
                    float durationZ;
                    Action onCompleteX;
                    Action onCompleteY;
                    Action onCompleteZ;
                    Action<float> setterX;
                    Action<float> setterY;
                    Action<float> setterZ;
                    Func<float, float> easeX;
                    Func<float, float> easeY;
                    Func<float, float> easeZ;

                    id1 = parseId();

                    startValue1 = parseVector3Value("start-value");
                    missingParamIfNullV3(startValue1);

                    endValue1 = parseVector3Value("end-value");
                    missingParamIfNullV3(endValue1);

                    float duration1 = parseFloatValue("duration");
                    if (duration1 != -114514) {
                        durationX = duration1;
                        durationY = duration1;
                        durationZ = duration1;
                    } else {
                        durationX = parseFloatValue("duration-x");
                        missingParamIfNull(durationX);
                        durationY = parseFloatValue("duration-y");
                        missingParamIfNull(durationY);
                        durationZ = parseFloatValue("duration-z");
                        missingParamIfNull(durationZ);
                    }

                    Action onComplete1 = parseOnComplete("on-complete");
                    if (onComplete1 != null) {
                        onCompleteX = onComplete1;
                        onCompleteY = onComplete1;
                        onCompleteZ = onComplete1;
                    } else {
                        onCompleteX = parseOnComplete("on-complete-x");
                        onCompleteY = parseOnComplete("on-complete-y");
                        onCompleteZ = parseOnComplete("on-complete-z");
                    }

                    Action<float> setter1 = parseSetter("setter");
                    if (setter1 != null) {
                        setterX = setter1;
                        setterY = setter1;
                        setterZ = setter1;
                    } else {
                        setterX = parseSetter("setter-x");
                        missingParamIfNull(setterX);
                        setterY = parseSetter("setter-y");
                        missingParamIfNull(setterY);
                        setterZ = parseSetter("setter-z");
                        missingParamIfNull(setterZ);
                    }

                    Func<float, float> ease1 = parseEaseValue("ease");
                    if (ease1 != null) {
                        easeX = ease1;
                        easeY = ease1;
                        easeZ = ease1;
                    } else {
                        easeX = parseEaseValue("ease-x");
                        missingParamIfNull(easeX);
                        easeY = parseEaseValue("ease-y");
                        missingParamIfNull(easeY);
                        easeZ = parseEaseValue("ease-z");
                        missingParamIfNull(easeZ);
                    }

                    if (tweenManager == null) 
                        tweenManager = GameObject.Find("Tween Manager").GetComponent<TweenManager>();
                    Modules.Tween.Scripts.Tween tweenX = new();
                    tweenX.setId(id1+"-x").setStartValue(startValue1.x).setEndValue(endValue1.x)
                        .setDuration(durationX).setOnComplete(onCompleteX).setSetter(setterX)
                        .setEasing(easeX);
                    Modules.Tween.Scripts.Tween tweenY = new();
                    tweenY.setId(id1+"-y").setStartValue(startValue1.y).setEndValue(endValue1.y)
                        .setDuration(durationY).setOnComplete(onCompleteY).setSetter(setterY)
                        .setEasing(easeY);
                    Modules.Tween.Scripts.Tween tweenZ = new();
                    tweenZ.setId(id1+"-z").setStartValue(startValue1.z).setEndValue(endValue1.z)
                        .setDuration(durationZ).setOnComplete(onCompleteZ).setSetter(setterZ)
                        .setEasing(easeZ);
                    
                    tweenManager.register(tweenX);
                    tweenManager.register(tweenY);
                    tweenManager.register(tweenZ);
                    break;
            }
        }

        private float parseFloatValue(string key) {
            if (!properties.ContainsKey(key))
                return -114514;
            else if (properties[key] is string strVar) 
                return float.Parse(strVar);
            else 
                throw new UnsupportedTweenException(UnsupportedTweenException.WRONG_PARAM_TYPE);
        }

        private Vector3 parseVector3Value(string key) {
            if (!properties.ContainsKey(key))
                return new Vector3(-114514, -114514, -114514);
            else if (properties[key] is string strVar) {
                string[] vectorComps = strVar.Split(",");
                Vector3 vector = new Vector3(
                    float.Parse(vectorComps[0].Trim()), 
                    float.Parse(vectorComps[1].Trim()), 
                    float.Parse(vectorComps[2].Trim()));
                return vector;
            } else if (properties[key] is Vector3 v) 
                return v;
            else
                throw new UnsupportedTweenException(UnsupportedTweenException.WRONG_PARAM_TYPE);
        }

        private Func<float, float> parseEaseValue(string key) {
            if (!properties.ContainsKey(key)) 
                throw null;
            if (properties[key] is string easeStr) {
                Ease easeInst = Ease.getEase(easeStr);
                if (easeInst == null)
                    throw new UnsupportedTweenException(UnsupportedTweenException.EASE_TYPE_NOT_FOUND);
                return x => easeInst.func(x);
            } else {
                if (properties[key] is Func<float, float> funcInst)
                    return funcInst;
                throw new UnsupportedTweenException(UnsupportedTweenException.WRONG_PARAM_TYPE);
            }
        }

        private string parseId() {
            if (!properties.ContainsKey("id")) 
                return System.Guid.NewGuid().ToString() + $"-{UnityEngine.Random.Range(0, 1000)}";
            else if (properties["id"] is string strVar) 
                return strVar;
            else 
                throw new UnsupportedTweenException(UnsupportedTweenException.WRONG_PARAM_TYPE);
        }

        private Action parseOnComplete(string key) {
            if (!properties.ContainsKey(key))
                return null;
            if (properties[key] is Action onComplete)
                return onComplete;
            else 
                throw new UnsupportedTweenException(UnsupportedTweenException.WRONG_PARAM_TYPE);
        }

        private Action<float> parseSetter(string key) {
            if (!properties.ContainsKey(key))
                return null;
            if (properties[key] is Action<float> setter)
                return setter;
            else 
                throw new UnsupportedTweenException(UnsupportedTweenException.WRONG_PARAM_TYPE);
        }

        private void missingParamIfNull(object obj) {
            if (obj == null)
                throw new UnsupportedTweenException(UnsupportedTweenException.MISSING_PARAM);
        }
        private void missingParamIfNullf(float obj) {
            if (obj == -114514)
                throw new UnsupportedTweenException(UnsupportedTweenException.MISSING_PARAM);
        }
        private void missingParamIfNullV3(Vector3 obj) {
            if (obj.x == -114514 && obj.y == -114514 && obj.z == -114514)
                throw new UnsupportedTweenException(UnsupportedTweenException.MISSING_PARAM);
        }
    }
}
