using NCMB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuickRanking : MonoBehaviour
{
    public string rankingClassName = "Ranking";//NCMB側のランキングクラス名//
    public int count = 10;//いくつまでランキングデータを取得するか//
    private List<RankingData> rankingDataList = new List<RankingData>();//取得したランキングデータ//
    public bool IsRankingDataValid { get; private set; }//ランキングデータの取得に成功しているか//

    public int PlayerCount { get; private set; }//いままで何人がスコアを登録したか//

    private string currentObjectid;//自分のスコアのidを一時保存する//

    public static QuickRanking Instance;//シングルトン//

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            CheckNCMBValid();
        }
    }

    public void FetchRanking(UnityAction callback = null)
    {
        if (CheckNCMBValid() == false)
        {
            if (callback != null) callback();
            return;
        }

        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(rankingClassName);

        //Scoreの値で降順にソート//
        query.OrderByDescending("Score");

        //取得数の設定//
        query.Limit = count;

        query.FindAsync((List<NCMBObject> objList, NCMBException e) =>
        {
            if (e != null)
            {
                //検索失敗時の処理
                IsRankingDataValid = false;
            }
            else
            {
                int num = 1;

                rankingDataList.Clear();

                foreach (NCMBObject obj in objList)
                {
                    rankingDataList.Add(new RankingData(
                         num++,
                         name: obj["Name"] as string,
                         score: Convert.ToInt32(obj["Score"]),
                         objectid: obj.ObjectId

                        ));
                }
                IsRankingDataValid = true;
            }

            if (callback != null) callback();
        });
    }

    public void SaveRanking(string name, int score, UnityAction callback = null)
    {
        //スコアがゼロなら登録しない//
        if (CheckNCMBValid() == false || score <= 0)
        {
            if (callback != null) callback();
            return;
        }

        //rankingClassNameに設定したオブジェクトを作る//
        NCMBObject ncmbObject = new NCMBObject(rankingClassName);

        //nameが空だったらNoNameと入れる//
        if (string.IsNullOrEmpty(name)) name = "No Name";

        // オブジェクトに値を設定
        ncmbObject["Name"] = name;
        ncmbObject["Score"] = score;

        // データストアへの登録
        ncmbObject.SaveAsync((NCMBException e) =>
        {
            if (e != null)
            {
                //接続失敗//
            }
            else
            {
                //接続成功//
                //保存したオブジェクトidを記録//
                currentObjectid = ncmbObject.ObjectId;
            }

            //ランキングの更新//
            if(callback != null)
            {
                FetchRanking(callback);
            }else
            {
                FetchRanking();
            }

        });
    }

    public List<RankingData> GetRanking()
    {
        //すでにStart()でフェッチ済みのデータを渡すだけ//
        return rankingDataList;
    }

    public string GetRankingByText()
    {
        if (IsRankingDataValid)
        {
            string text = string.Empty;

            foreach (RankingData rankingData in rankingDataList)
            {
                string rankNum = string.Format("{0, 2}", rankingData.rankNum);
                string name = string.Format("{0, -10}", rankingData.name);
                string score = string.Format("{0, -10}", rankingData.score.ToString());

                //さっき保存したスコアがあった場合は赤に着色する//
                if (rankingData.objectid == currentObjectid)
                {
                    text += "<color=red>" + rankNum + ": \t" + name + ": \t" + score + "</color> \n";
                }
                else
                {
                    text += rankNum + ": \t" + name + ": \t" + score + "\n";
                }
            }

            return text;
        }
        else
        {
            return "No Ranking Data";
        }
    }

    public void FetchPlayerCount(UnityAction callback = null)
    {
        if (CheckNCMBValid() == false)
        {
            if (callback != null) callback();
            return;
        }

        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(rankingClassName);
        query.CountAsync((int count, NCMBException e) => {
            if (e != null)
            {
                //接続失敗//
            }
            else
            {
                //接続成功//
                PlayerCount = count;
            }
            if (callback != null) callback();
        });
    }

    private bool CheckNCMBValid()
    {
#if UNITY_WEBGL
            Debug.LogWarning("NCMB SDK はWebGLに対応していません。");
            return false;
#else
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("ネットワーク接続がありません。");
            return false;
        }
        else
        {
            return true;
        }
#endif
    }
}

public class RankingData
{
    public readonly int rankNum;//順位（本クラス内でつける）//
    public readonly string name;//プレイヤー名//
    public readonly int score;//点数//
    public readonly string objectid;//NCMBのオブジェクトID//

    public RankingData(int rankNum, string name, int score, string objectid)
    {
        this.rankNum = rankNum;
        this.name = name;
        this.score = score;
        this.objectid = objectid;
    }
}