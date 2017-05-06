using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sample : MonoBehaviour {
	public Text scoreBoardText;
	public Text playerCounter;
	int counter = 0;
	string rank;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(int.Parse(QuickRanking.Instance.PlayerCount.ToString()) != 0 && counter == 0){
			counter++;
			Debug.Log("Total Player Count: "+ QuickRanking.Instance.PlayerCount.ToString());
			rank = QuickRanking.Instance.GetRankingByText();
			Debug.Log (rank);
		}
	}

	public void SaveScore(){
		QuickRanking.Instance.SaveRanking("hogehoge", 2243);
	}

	public void Ranking(){
		QuickRanking.Instance.FetchRanking();
		QuickRanking.Instance.FetchPlayerCount();


	}
}
