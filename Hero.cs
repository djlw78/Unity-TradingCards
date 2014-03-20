using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {

	public int health = 30;
	public int damage = 0;
	
	#region visual
	public TextMesh healthText;
	public Texture2D image;
	#endregion
	
	void OnMouseDown()
	{
		if(Board.instance.currentCard)
		{
			if(Board.instance.currentCard.canPlay)
			{
				Board.instance.currentCard.AttackHero(Board.instance.currentCard, this, delegate {
					Board.instance.currentCard.canPlay = false;
				});
			}
		}
	}
	
	void Update()
	{
		healthText.text = health.ToString();
	}
}
