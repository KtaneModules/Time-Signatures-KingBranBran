using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TimeSigModule : MonoBehaviour {
	// Use this for initialization

	public KMBombModule module;
	public KMAudio moduleAudio;
	public KMSelectable topButton;
	public KMSelectable bottomButton;

	public GameObject buttonBack;
	public TextMesh topButtonText;
	public TextMesh bottomButtonText;

	private string topButtonStates = "123456789";
	private string bottomButtonStates = "1248";
	private string currentState = "##";
	private string redNumber = "#";

	private string[] randomSequence = new string[5];
	private int amountCorrect = 0;

	private static int _moduleIdCounter = 1;
	private int _moduleId;

	private bool moduleSolved = false;

	private Coroutine buttonHold;
	private bool holding = false;

	private Coroutine playingSound;

	private 

	void Awake()
	{
		_moduleId = _moduleIdCounter++;
		buttonBack.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, .6f, 1f, .4f, 1f);
	}

	void Start () {
		currentState = topButtonStates[Random.Range(0, 9)].ToString() + bottomButtonStates[Random.Range(0, 4)].ToString(); // Pick two random numbers
		GenerateColor();
		UpdateText();

		topButton.OnInteract += delegate { ButtonPressed(0); return false; };
		bottomButton.OnInteract += delegate { ButtonPressed(1); return false; };

		topButton.OnInteractEnded += delegate { ButtonDepressed(0); };
		bottomButton.OnInteractEnded += delegate { ButtonDepressed(1); };
	}

	void ButtonPressed(int buttonNum)
	{
		moduleAudio.PlaySoundAtTransform("Click", module.transform);
		if (moduleSolved) return;

		if (buttonHold != null)
		{
			holding = false;
			StopCoroutine(buttonHold);
			buttonHold = null;
		}

		buttonHold = StartCoroutine(HoldChecker());
	}

	void ButtonDepressed(int buttonNum)
	{
		moduleAudio.PlaySoundAtTransform("ClickOff", module.transform);
		if (moduleSolved ) return;

		StopCoroutine(buttonHold);

		if (holding)
		{
			SubmitPressed(buttonNum);
		}
		else
		{
			if (buttonNum == 0)
			{
				if (currentState[0].ToString() == redNumber)
				{
					StopPlaying();

					playingSound = StartCoroutine(PlayRandomSequence());
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
			UpdateText();
		}	
	}

	IEnumerator HoldChecker()
	{
		yield return new WaitForSeconds(.6f);
		holding = true;
		UpdateDisplayTo("  ");
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

	void UpdateText()
	{
		topButtonText.text = currentState[0].ToString();
		bottomButtonText.text = currentState[1].ToString();

		topButtonText.color = topButtonText.text == redNumber ? new Color(.78f, 0, 0) : new Color(0, 0, 0);
	}

	void GenerateColor() {
		var redNum = topButtonStates[Random.Range(0, topButtonStates.Length)].ToString();
		redNumber = redNum;
		DebugLog("The red number is now {0}.", redNum);
	}

	private void SubmitPressed(int buttonNum)
	{
		if (currentState == randomSequence[amountCorrect])
		{
			amountCorrect++;
			UpdateText();
			StartCoroutine(PlayCorrectSound());
		}
		else
		{
			module.HandleStrike();
			StopPlaying();
			GenerateColor();
			UpdateText();
		}
		if (amountCorrect == 5) StartCoroutine(ModuleSolve());
	}

	void UpdateDisplayTo(string display)
	{
		topButtonText.text = display[0].ToString();
		bottomButtonText.text = display[1].ToString();
	}

	void StopPlaying()
	{
		if (playingSound != null)
		{
			StopCoroutine(playingSound);
			playingSound = null;
		}
	}

	IEnumerator ModuleSolve()
	{
		StopPlaying();
		moduleSolved = true;
		topButtonText.color = new Color(0, 0, 0);
		bottomButtonText.color = new Color(0, 0, 0);
		UpdateDisplayTo("  ");
		yield return new WaitForSeconds(1f);

		for (int i = 8; i > 0; i--)
		{
			moduleAudio.PlaySoundAtTransform("EmphasizedTap", module.transform);
			UpdateDisplayTo(i.ToString() + "8");
			yield return new WaitForSeconds(.2f);

			for (int j = 0; j < i - 1; j++) 
			{
				moduleAudio.PlaySoundAtTransform("Tap", module.transform);
				yield return new WaitForSeconds(.2f);
			}
		}		
		moduleAudio.PlaySoundAtTransform("EmphasizedTap", module.transform);
		yield return new WaitForSeconds(.2f);
		moduleAudio.PlaySoundAtTransform("EmphasizedTap", module.transform);
		yield return new WaitForSeconds(.2f);
		moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
		UpdateDisplayTo("TS");
		yield return new WaitForSeconds(.3f);
		moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
		UpdateDisplayTo("II");
		yield return new WaitForSeconds(.3f);
		moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
		UpdateDisplayTo("MG");
		yield return new WaitForSeconds(.2f);
		moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
		UpdateDisplayTo("EN");
		yield return new WaitForSeconds(.4f);
		moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
		UpdateDisplayTo("  ");
		yield return new WaitForSeconds(.4f);
		moduleAudio.PlaySoundAtTransform("EmphasizedTap", module.transform);
		moduleAudio.PlaySoundAtTransform("Ding", module.transform);
		topButtonText.color = new Color(0, .58f, 0);
		bottomButtonText.color = new Color(0, .58f, 0);
		UpdateDisplayTo("✓✓");
		module.HandlePass();
	}

	IEnumerator PlayRandomSequence()
	{
		// Make the sequence
		for (int i = 0; i < randomSequence.Length; i++)
			randomSequence[i] = topButtonStates[Random.Range(0, 9)].ToString() + bottomButtonStates[Random.Range(0, 4)].ToString();

		DebugLog("The sequence is: " + randomSequence[0] + ", " + randomSequence[1] + ", " + randomSequence[2] + ", " + randomSequence[3] + ", " + randomSequence[4]);

		// Play the sequence
		for (int i = 0; i < randomSequence.Length; i++)
		{
			var topNum = int.Parse(randomSequence[i][0].ToString());
			var bottomNum = int.Parse(randomSequence[i][1].ToString());
			var bps = Random.Range(.25f, .375f);
			var numOfTaps = topNum;
			
			switch (bottomNum)
			{
				case 1:
					numOfTaps  *= 8;
					break;
				case 2:
					numOfTaps  *= 4;
					break;
				case 4:
					numOfTaps  *= 2;
					break;
				default:
					break;
			}

			for (int j = 0; j < numOfTaps; j++)
			{
				if (j == 0)
				{
					moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
				}
				else if (bottomNum == 8 || bottomNum == 4 && j % 2 == 0 || bottomNum == 2 && j % 4 == 0 || bottomNum == 1 && j % 8 == 0)
				{
					moduleAudio.PlaySoundAtTransform("EmphasizedTap", module.transform);
				}
				else
				{
					moduleAudio.PlaySoundAtTransform("Tap", module.transform);
				}

				yield return new WaitForSeconds(bps);
			}
		}
	}

	IEnumerator PlayCorrectSound()
	{
		moduleAudio.PlaySoundAtTransform("EmphasizedTap", module.transform);
		yield return new WaitForSeconds(.1f);
		moduleAudio.PlaySoundAtTransform("HighIshTap", module.transform);
		yield return new WaitForSeconds(.1f);
		moduleAudio.PlaySoundAtTransform("HighTap", module.transform);
	}

	private void DebugLog(string log, params object[] args)
	{
		var logData = string.Format(log, args);
		Debug.LogFormat("[Time Signatures #{0}] {1}", _moduleId, logData);
	}
}
