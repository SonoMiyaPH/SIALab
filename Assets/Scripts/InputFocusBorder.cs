using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Lights up an input field's border while it is selected.</summary>
[RequireComponent(typeof(TMP_InputField))]
public class InputFocusBorder : MonoBehaviour
{
	public Image border;
	public Color idleColor = new Color(0.25f, 0.71f, 0.76f, 0f);
	public Color focusColor = new Color(0.25f, 0.71f, 0.76f, 1f);

	void Awake()
	{
		var input = GetComponent<TMP_InputField>();
		input.onSelect.AddListener(_ => SetFocus(true));
		input.onDeselect.AddListener(_ => SetFocus(false));
		SetFocus(false);
	}

	void SetFocus(bool focused)
	{
		if (border != null) border.color = focused ? focusColor : idleColor;
	}
}