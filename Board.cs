using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour, ICard {
	
	public static Board instance;
	
	public List<GameObject> friendlyDeckCards = new List<GameObject>();
	public List<GameObject> opponentDeckCards = new List<GameObject>();
	public List<GameObject> friendlyHandCards = new List<GameObject>();
	public List<GameObject> opponentHandCards = new List<GameObject>();
	public List<GameObject> friendlyTableCards = new List<GameObject>();
	public List<GameObject> opponentTableCards = new List<GameObject>();
	
	public Transform friendlyDeckPos;
	public Transform opponentDeckPos;
	public Transform friendlyHandPos;
	public Transform opponentHandPos;
	public Transform friendlyTablePos;
	public Transform opponentTablePos;
	
	public TextMesh friendlyManaText;
	public TextMesh opponentManaText;
	
	public Hero friendlyHero;
	public Hero opponentHero;
	
	public enum Turn { Friendly, Opponent };
	public Turn turn = Turn.Friendly;
	
	int maxMana = 1;
	int friendlyMana = 1;
	int opponentMana = 1;
	
	public bool gameStarted = false;
	int turnNumber = 1;
	
	public Card.CardEvent CardChanged;
	
	public delegate void BoardUpdate();
	public event BoardUpdate UpdateBoard;
	
	public Card currentCard;
	public Card targetCard;
	
	public List<Hashtable> boardHistory = new List<Hashtable>();
	
	#region audio
	public AudioClip cardDrop;
	public AudioClip cardDestroy;
	#endregion
	
	public void AddHistory(Card a, Card b)
	{
		Hashtable hash = new Hashtable();
		
		hash.Add(a, b);
		
		boardHistory.Add(hash);
	}
	
	void Awake()
	{
		instance = this;
	}
	
	void Start()
	{
		#region binds
		CardChanged += PositionUpdate;
		#endregion
		
		foreach(GameObject CardObject in GameObject.FindGameObjectsWithTag("Card"))
		{			
			Card c = CardObject.GetComponent<Card>();
			
			if(c.team == Card.Team.Friendly)
				friendlyDeckCards.Add(CardObject);
			else
				opponentDeckCards.Add(CardObject);
		}
		
		PositionUpdate();
		PresentHand();
		
		StartGame();
	}
	
	public void StartGame()
	{
		gameStarted = true;
		OnUpdate();
		
		for(int i = 0; i<3; i++)
		{
			DrawCardFromDeck(Card.Team.Friendly);
			DrawCardFromDeck(Card.Team.Opponent);
		}
	}
	
	public void DrawCardFromDeck(Card.Team team)
	{

		if(team == Card.Team.Friendly && friendlyDeckCards.Count != 0 && friendlyHandCards.Count < 10)
		{
			int random = Random.Range(0, friendlyDeckCards.Count);
			GameObject tempCard = friendlyDeckCards[random];
			
			//tempCard.transform.position = friendlyHandPos.position;
			tempCard.GetComponent<Card>().newPos = friendlyHandPos.position;
			tempCard.GetComponent<Card>().SetCardStatus(Card.CardStatus.InHand);
			
			friendlyDeckCards.Remove(tempCard);
			friendlyHandCards.Add(tempCard);
		}
		
		if(team == Card.Team.Opponent && opponentDeckCards.Count != 0  && opponentHandCards.Count < 10)
		{
			int random = Random.Range(0, opponentDeckCards.Count);
			GameObject tempCard = opponentDeckCards[random];
			
			tempCard.transform.position = opponentHandPos.position;
			tempCard.GetComponent<Card>().SetCardStatus(Card.CardStatus.InHand);
			
			opponentDeckCards.Remove(tempCard);
			opponentHandCards.Add(tempCard);
		}
		
		OnUpdate();
		PresentHand();
	}
	
	public void PlaceRandomCard(Card.Team team)
	{		
		if(team == Card.Team.Friendly && friendlyHandCards.Count != 0)
		{
			int random = Random.Range(0, friendlyHandCards.Count);
			GameObject tempCard = friendlyHandCards[random];
			
			PlaceCard(tempCard.GetComponent<Card>());
		}
		
		if(team == Card.Team.Opponent && opponentHandCards.Count != 0)
		{
			int random = Random.Range(0, opponentHandCards.Count);
			GameObject tempCard = opponentHandCards[random];
			
			PlaceCard(tempCard.GetComponent<Card>());
		}
		
		OnUpdate();
		EndTurn();
		
		PresentTable();
		PresentHand();
	}
	
	public void PlaceCard(Card card)
	{
		if(card.team == Card.Team.Friendly && friendlyMana - card.mana >= 0 && friendlyTableCards.Count < 10)
		{
			//card.gameObject.transform.position = friendlyTablePos.position;
			card.GetComponent<Card>().newPos = friendlyTablePos.position;
			
			friendlyHandCards.Remove(card.gameObject);
			friendlyTableCards.Add(card.gameObject);
			
			card.SetCardStatus(Card.CardStatus.OnTable);
			PlaySound(cardDrop);
			
			friendlyMana -= card.mana;
		}
		
		if(card.team == Card.Team.Opponent && opponentMana - card.mana >= 0 && opponentTableCards.Count < 10)
		{
			//card.gameObject.transform.position = opponentTablePos.position;
			card.GetComponent<Card>().newPos = opponentTablePos.position;
			
			opponentHandCards.Remove(card.gameObject);
			opponentTableCards.Add(card.gameObject);
			
			card.SetCardStatus(Card.CardStatus.OnTable);
			PlaySound(cardDrop);
			
			opponentMana -= card.mana;
		}
		
		PresentTable();
		PresentHand();
		OnUpdate();
	}
	
	#region positioning
	public void PresentHand()
	{
		float space = 0f;
		float space2 = 0f;
		float gap = 3;
		
		foreach(GameObject card in friendlyHandCards)
		{
			int numberOfCards = friendlyHandCards.Count;
			//card.transform.position = friendlyHandPos.position + new Vector3(-numberOfCards + space - 2,0,0);
			card.GetComponent<Card>().newPos = friendlyHandPos.position + new Vector3(-numberOfCards + space - 2,0,0);
			space += gap;
		}
		
		foreach(GameObject card in opponentHandCards)
		{
			int numberOfCards = opponentHandCards.Count;
			//card.transform.position = opponentHandPos.position + new Vector3(-numberOfCards + space2 - 2,0,0);
			card.GetComponent<Card>().newPos = opponentHandPos.position + new Vector3(-numberOfCards + space2 - 2,0,0);
			space2 += gap;
		}
	}
	
	public void PresentTable()
	{
		float space = 0f;
		float space2 = 0f;
		float gap = 3;
		
		foreach(GameObject card in friendlyTableCards)
		{
			int numberOfCards = friendlyTableCards.Count;
			//card.transform.position = friendlyTablePos.position + new Vector3(-numberOfCards + space - 2,0,0);
			card.GetComponent<Card>().newPos = friendlyTablePos.position + new Vector3(-numberOfCards + space - 2,0,0);
			space += gap;
		}
		
		foreach(GameObject card in opponentTableCards)
		{
			int numberOfCards = opponentTableCards.Count;
			//card.transform.position = opponentTablePos.position + new Vector3(-numberOfCards + space2,0,0);
			card.GetComponent<Card>().newPos = opponentTablePos.position + new Vector3(-numberOfCards + space2,0,0);
			space2 += gap;
		}
	}
	
	void PositionUpdate()
	{
		foreach(GameObject CardObject in friendlyDeckCards)
		{
			Card c = CardObject.GetComponent<Card>();
			
			if(c.cardStatus == Card.CardStatus.InDeck)
			{
				//CardObject.transform.position = friendlyDeckPos.position + new Vector3(Random.value, Random.value, Random.value);
				c.newPos = friendlyDeckPos.position + new Vector3(Random.value, Random.value, Random.value);
			}
		}
		
		foreach(GameObject CardObject in opponentDeckCards)
		{
			Card c = CardObject.GetComponent<Card>();
			
			if(c.cardStatus == Card.CardStatus.InDeck)
			{
				//CardObject.transform.position = opponentDeckPos.position + new Vector3(Random.value, Random.value, Random.value);
				c.newPos = opponentDeckPos.position + new Vector3(Random.value, Random.value, Random.value);
			}
		}
	}
	#endregion
	
	void OnUpdate()
	{
		friendlyManaText.text = friendlyMana.ToString() + "/" + maxMana;
		opponentManaText.text = opponentMana.ToString() + "/" + maxMana;
		
		if(friendlyHero.health <= 0)
			EndGame(opponentHero);
		if(opponentHero.health <= 0)
			EndGame(friendlyHero);
		
		//UpdateBoard();
	}
	
	void OnNewTurn()
	{
		OnUpdate();
		//UpdateBoard();
		
		if(turn == Turn.Opponent)
			OpponentAI();
	}
	
	void OpponentAI()
	{
		if(turn == Turn.Opponent)
		{
			Hashtable attacks = new Hashtable();
			
			#region attacking
			foreach(GameObject Card in opponentTableCards)
			{
				Card card = Card.GetComponent<Card>();
				
				if(card.canPlay)
				{
					float changeToAttackhero = Random.value;
					
					if(changeToAttackhero < 0.50f)
					{
						card.AttackHero(card, friendlyHero, delegate {
							card.canPlay = false;
						});
					}
					else if(friendlyTableCards.Count > 0)
					{
						int random = Random.Range(0, friendlyTableCards.Count);
						GameObject CardToAttack = friendlyTableCards[random];
						
						attacks.Add(card, CardToAttack.GetComponent<Card>());
					}
				}
			}
			
			foreach(DictionaryEntry row in attacks)
			{
				Card tempCard = row.Key as Card;
				Card temp2 = row.Value as Card;
				
				tempCard.Attack(tempCard, temp2, delegate {
					tempCard.canPlay = false;
				});
			}
			#endregion
			
			#region placing cards
			float chanceToPlace = Random.value;
			
			if(opponentHandCards.Count == 0)
			{
				EndTurn();
			}
			else
			{
				PlaceRandomCard(Card.Team.Opponent);
			}
			#endregion
		}
	}//random
	
	void EndTurn()
	{
		maxMana += 1;
		if(maxMana >= 10) maxMana = 10;
		
		friendlyMana = maxMana;
		opponentMana = maxMana;

		turnNumber += 1;
		
		currentCard = null;
		targetCard = null;
		
		foreach(GameObject card in friendlyTableCards)
			card.GetComponent<Card>().canPlay = true;
		
		foreach(GameObject card in opponentTableCards)
			card.GetComponent<Card>().canPlay = true;
		
		if(turn == Turn.Opponent)
		{
			DrawCardFromDeck(Card.Team.Opponent);
			turn = Turn.Friendly;
		}
		else if(turn == Turn.Friendly)
		{
			DrawCardFromDeck(Card.Team.Friendly);
			turn = Turn.Opponent;
		}
		
		PresentHand();
		PresentTable();
		
		OnNewTurn();
	}
	
	void EndGame(Hero winner)
	{
		if(winner == friendlyHero)
		{
			
		}
		
		if(winner == opponentHero)
		{
			
		}
	}
	
	void OnGUI()
	{
		if(gameStarted)
		{
			if(turn == Turn.Friendly)
			{
				if(GUI.Button(new Rect(Screen.width - 200,Screen.height/2 - 50, 100, 50), "End Turn"))
				{
					EndTurn();
				}
			}
			
			GUI.Label(new Rect(0,0,100,50), "Turn: " + turn + " Turn Number: " + turnNumber.ToString());
			
			foreach(Hashtable history in boardHistory)
			{
				foreach(DictionaryEntry entry in history)
				{
					Card card1 = entry.Key as Card;
					Card card2 = entry.Value as Card;
					
					GUILayout.Label(card1.name + " > " + card2.name);
				}
			}
		}
	}
	
	public void PlaySound(AudioClip sound)
	{
		audio.PlayOneShot(sound);
	}
}
