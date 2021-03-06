using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIPanel))]
public class BCombatMenu : MonoBehaviour {
	public GameObject attackButtonPrefab;
	public float attackButtonHeight;

	public GameObject gameoverPanel;
	public UILabel gameoverLabel;
	public GameObject attackRing;
	public UIButton backButton;
	public UIButton endTurnButton;
	public BNotification bNotification;

	public AudioSource loseSound;
	public AudioSource winSound;

	BUnit bUnit;

	public void OpenForBUnit(BUnit bUnit) 
	{
		this.bUnit = bUnit;
		if(bUnit.unit.AIControled) {
			attackRing.SetActive(false);
			backButton.gameObject.SetActive(false);
		} else {
			if(bUnit.unit.CanAttack) {
				attackRing.SetActive(true);
			}
			endTurnButton.gameObject.SetActive(true);
			backButton.gameObject.SetActive(false);

			// clear attack ring ui
			List<GameObject> atkButtons = new List<GameObject>();
			foreach(Transform child in attackRing.transform) atkButtons.Add(child.gameObject);
			foreach(GameObject child in atkButtons) Destroy(child);
			atkButtons.Clear();

			// init attack buttons
			foreach(Attack atk in bUnit.unit.AttacksArray) {
				GameObject handle = (GameObject) Instantiate(attackButtonPrefab);
				handle.transform.parent = attackRing.transform;

				handle.GetComponent<BAttackButton>().Init(atk,this);
				atkButtons.Add(handle);
			}

			// arrange attack buttons
			RepositionAtkButtons(atkButtons);
		}
	}

	void RepositionAtkButtons(List<GameObject> atkButtons)
	{
		for (int i = 0; i < atkButtons.Count; i++) {
			atkButtons[i].transform.localPosition = new Vector3(0, -i*attackButtonHeight);
			atkButtons[i].transform.localScale = Vector3.one;
		}
		endTurnButton.transform.localPosition = new Vector3(452, -atkButtons.Count*attackButtonHeight);
		endTurnButton.transform.localScale = Vector3.one;
	}

	public void ChooseAttack()
	{
		backButton.gameObject.SetActive(true);
		endTurnButton.gameObject.SetActive(false);
		attackRing.SetActive(true);
	}
	
	public void ActionAttack(Attack attack) 
	{
		attackRing.SetActive(false);
		backButton.gameObject.SetActive(true);
		endTurnButton.gameObject.SetActive(false);
		bUnit.ClearDisplayRange();
		bUnit.DisplayAttackRange(attack);
	}
	
	public void ActionMove() 
	{
		backButton.gameObject.SetActive(true);
		bUnit.DisplayMovementRange();
	}

	public void ActionEndTurn()
	{
		Hide();
		bUnit.EndTurn();
	}

	public void Back()
	{
		backButton.gameObject.SetActive(false);
		attackRing.SetActive(true);
		endTurnButton.gameObject.SetActive(true);
		bUnit.ClearDisplayRange();
		bUnit.DisplayMovementRange();
	}

	public void ActionCompleted()
	{
		OpenForBUnit (bUnit);
	}
	
	public void Hide() 
	{
		attackRing.SetActive(false);
		backButton.gameObject.SetActive(false);
		endTurnButton.gameObject.SetActive(false);
	}

	public void DisplayGameover(string text, bool playerWin)
	{
		gameoverPanel.SetActive(true);
		gameoverLabel.text = text;
		if(playerWin) {
			winSound.Play();
		} else {
			loseSound.Play();
		}
	}

	public void ShowTurnStart(BUnit bUnit)
	{
		StartCoroutine(NotifyRoutine(bUnit));
	}

	IEnumerator NotifyRoutine(BUnit bUnit)
	{
		if(bUnit.unit.team == Unit.Team.PLAYER) {
			bNotification.Display("Dein " + bUnit.unit.UnitName + " ist am Zug");
		} else {
			bNotification.Display("Gegnerische " + bUnit.unit.UnitName + " ist am Zug");
		}
		yield return new WaitForSeconds(2f);
		EventProxyManager.FireEvent(this, new EventDoneEvent());
	}
}