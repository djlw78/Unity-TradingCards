using UnityEngine;
using System.Collections;

[System.Serializable]
public class Card : MonoBehaviour, ICard
{
	public string name = "Card";
	public string description = "Description";
	public Texture2D image;
	public int health;
	public int damage;
	public int mana;
	
	public bool canPlay = false;
	
	public enum CardStatus { InDeck, InHand, OnTable, Destroyed };
	public CardStatus cardStatus = CardStatus.InDeck;
	
	public enum Team { Friendly, Opponent };
	public Team team = Team.Friendly;
	
	public delegate void CardEvent();
	public event CardEvent Changed;
	
	public delegate void CustomAction();
	
	#region visual
	public TextMesh healthText;
	public TextMesh damageText;
	public TextMesh manaText;
	public TextMesh debugText;
	#endregion
	
	public Vector3 newPos;
	
	void Update()
	{
		transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 20);
		
		healthText.text = health.ToString();
		damageText.text = damage.ToString();
		manaText.text = mana.ToString();
		
		debugText.text = canPlay ? "Ready to attack" : "Nope";
	}
	
	void Start()
	{
		health = Random.Range(0,8);
		damage = Random.Range(0,8);
		mana = Random.Range(0,8);
		
		CardUpdated();
	}
	
	public void SetCardStatus(CardStatus status)
	{
		cardStatus = status;
	}
	
	public void Attack(Card attacker, Card target, CustomAction action)
	{
		target.health -= attacker.damage;
		attacker.health -= target.damage;
		
		if(target.health <= 0)
		{
			Destroy(target);
		}
		
		if(attacker.health <= 0)
		{
			attacker.Destroy(attacker);
		}
		
		action();
	}
	
	public void CardUpdated()
	{
		//binded to Changed event
		//when card values are changed
		
		healthText.text = health.ToString();
		damageText.text = damage.ToString();
		manaText.text = mana.ToString();
	}
	
	void OnMouseDown()
	{
		if(Board.instance.turn == Board.Turn.Friendly && cardStatus == CardStatus.InHand && team == Team.Friendly)
		{
			Board.instance.PlaceCard(this);
		}
		
		if(Board.instance.turn == Board.Turn.Friendly && cardStatus == CardStatus.OnTable && team == Team.Friendly)
		{
			//clicked on friendly card on table to attack another table card
			Board.instance.currentCard = this;
			print ("Selected card: " + damage + ":" + health);
		}
		
		if(Board.instance.turn == Board.Turn.Friendly && cardStatus == CardStatus.OnTable && team == Team.Opponent)
		{
			//clicked opponent card on table on your turn
			if(Board.instance.currentCard != null && canPlay)
			{
				Board.instance.targetCard = this;

				if(Board.instance.currentCard.canPlay)
				{
					Attack (Board.instance.currentCard, Board.instance.targetCard, delegate {
						Board.instance.currentCard.canPlay = false;
					});
				} else print ("Card cannot attack");
			}
		}
	}
	
	void Destroy(Card card)
	{
		if(card.team == Card.Team.Friendly)
			Board.instance.friendlyTableCards.Remove(card.gameObject);
		else if(card.team == Card.Team.Opponent)
			Board.instance.opponentTableCards.Remove(card.gameObject);
		
		Destroy(card.gameObject);
		
		Board.instance.PresentTable();
	}
}
