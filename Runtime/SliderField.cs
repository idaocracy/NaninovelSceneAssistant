using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using NaninovelSceneAssistant;
using TMPro;

public class SliderField : ScriptableSlider
{
    private CommandParam param;
    private string buttonText;

    public override Slider UIComponent => GetComponentInChildren<Slider>();

    private Button button;

    Action<float> floatChange;

    public void Init(string Id, CommandParam param)
    {
        button.GetComponentInChildren<TextMeshProUGUI>().text = Id;
        OnSliderValueChanged += (v) => param.SetValue(v);
    }

    protected override void Awake()
    {
        base.Awake();
        button = GetComponentInChildren<Button>();
    }



}
