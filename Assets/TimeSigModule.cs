using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TimeSigModule : MonoBehaviour {
	// Use this for initialization

	public KMBombModule module;
	public KMSelectable topButton;
	public KMSelectable bottomButton;

	public GameObject buttonBack;
	public TextMesh topButtonText;
	public TextMesh bottomButtonText;

	private string topButtonStates = "123456789";
	private string bottomButtonStates = "1248";
	private string currentState = "##";
	private string redNumber = "#";
	private string greenNumber = "#";

	private string[] randomSequence = new string[10];

	private static int _moduleIdCounter = 1;
	private int _moduleId;

	private 

	void Awake()
	{
		_moduleId = _moduleIdCounter++;
		buttonBack.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, .6f, 1f, .4f, 1f);
	}

	void Start () {

		currentState = topButtonStates[Random.Range(0, 9)].ToString() + bottomButtonStates[Random.Range(0, 4)].ToString(); // Pick two random numbers
		GenerateColors();
		UpdateColors();

		topButton.OnInteract += delegate { ButtonPressed(0); return false; };
		bottomButton.OnInteract += delegate { ButtonPressed(1); return false; };
	}

	void ButtonPressed(int buttonNum)
	{
		if (buttonNum == 0)
		{
			if (currentState[0].ToString() == redNumber)
			{
				PlayRandomSequence();
			}
			else
			{
				CycleBottomScreen();
			}	
		}
		else
		{
			CycleTopScreen();
		}
		UpdateColors();
	}

	void CycleTopScreen()
	{
		var newText = int.Parse(currentState[0].ToString()) % 9 + 1;
		currentState = newText + currentState[1].ToString();
	}

	void CycleBottomScreen()
	{
		var newText = int.Parse(currentState[1].ToString()) * 2 % 15;
		currentState = currentState[0].ToString() + newText;
	}

	void UpdateColors()
	{
		var topSet = false;

		topButtonText.text = currentState[0].ToString();
		topButtonText.color = topButtonText.text == redNumber ? new Color(.78f, 0, 0) : new Color(0, 0, 0);

		if (topButtonText.text == redNumber)
			topSet = true;

		bottomButtonText.text = currentState[1].ToString();
		if (greenNumber.Length == 1 && !topSet)
		{
			topButtonText.color = topButtonText.text == greenNumber ? new Color(0, .58f, 0) : new Color(0, 0, 0);
		}	
		else if (greenNumber.Length > 1)
		{
			bottomButtonText.color = bottomButtonText.text == greenNumber[0].ToString() ? new Color(0, .58f, 0) : new Color(0, 0, 0);
		}	
	}

	void GenerateColors() {
		var redNum = topButtonStates[Random.Range(0, topButtonStates.Length)].ToString();
		var greenNum = Random.Range(0, 2) == 0 ?
			topButtonStates.Where(x => x.ToString() != redNum).ToArray()[Random.Range(0, topButtonStates.Length - 1)].ToString() :
			bottomButtonStates[Random.Range(0, bottomButtonStates.Length)] + " (Bottom)";

		redNumber = redNum;
		greenNumber = greenNum;

		DebugLog("The red and green numbers are {0} and {1} respectively.", redNum, greenNum);
	}

	void PlayRandomSequence()
	{
		for (int i = 0; i < randomSequence.Length; i++)
		{
			var attempts = 0;

			while (attempts < 1000) // Be safe xd
			{
				randomSequence[i] = topButtonStates[Random.Range(0, 9)].ToString() + bottomButtonStates[Random.Range(0, 4)].ToString();

				if (greenNumber.Length == 1 || (greenNumber.Length > 1 && randomSequence[i] != redNumber.ToString() + greenNumber[0].ToString()))
				{
					break;
				}
				attempts++;
			}

			if (attempts == 1000)
			{
				DebugLog("Wow.... I'm just gonna solve the module...");
				module.HandlePass();
			}

			DebugLog("#{0} is {1}", i + 1, randomSequence[i]);
		}
	}

	private void DebugLog(string log, params object[] args)
	{
		var logData = string.Format(log, args);
		Debug.LogFormat("[Time Signatures #{0}] {1}", _moduleId, logData);
	}
}
