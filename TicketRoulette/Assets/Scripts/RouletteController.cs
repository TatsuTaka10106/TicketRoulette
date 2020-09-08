using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.IO;

public class RouletteController : MonoBehaviour {
    [HideInInspector] public GameObject roulette;
    [HideInInspector] public float rotatePerRoulette;
    [HideInInspector] public RouletteMaker rMaker;
    [SerializeField] private bool isIkasama;
    [SerializeField] private int ikasamaID;
    private string result;
    private float rouletteSpeed;
    private float slowDownSpeed;
    private int frameCount;
    private bool isPlaying;
    private bool isStop;
    [SerializeField] private Text resultText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button retryButton;

    const string countKey = "counter";
    int count = 1;

    public void SetRoulette () {
        isPlaying = false;
        isStop = false;
        startButton.gameObject.SetActive (true);
        stopButton.gameObject.SetActive (false);
        retryButton.gameObject.SetActive(false);
        startButton.onClick.AddListener (StartOnClick);
        stopButton.onClick.AddListener (StopOnClick);
        retryButton.onClick.AddListener (RetryOnClick);
    }

    private void Update () {
        if (!isPlaying) return;
        roulette.transform.Rotate (0, 0, rouletteSpeed);
        frameCount++;
        if (isStop && frameCount > 3) {
            rouletteSpeed *= slowDownSpeed;
            slowDownSpeed -= 0.0001f;
            frameCount = 0;
        }
        if (rouletteSpeed < 0.05f) {
            isPlaying = false;
            ShowResult (roulette.transform.eulerAngles.z);
        }
    }

    private void StartOnClick () {
        Random.InitState(System.DateTime.Now.Millisecond);
        rouletteSpeed = Random.Range (12f, 13f);
        startButton.gameObject.SetActive (false);
        Invoke ("ShowStopButton", 1.5f);
        isPlaying = true;


        count = PlayerPrefs.GetInt(countKey, 1);
    }

    private void StopOnClick () {
        slowDownSpeed = Random.Range (0.97f, 0.99f);
        isStop = true;
        stopButton.gameObject.SetActive (false);
    }

    private void RetryOnClick(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowStopButton () {
        stopButton.gameObject.SetActive (true);
    }

    private void ShowResult(float x) {
        for (int i = 1; i <= rMaker.choices.Count; i++) {
            if (((rotatePerRoulette * (i - 1) <= x) && x <= (rotatePerRoulette * i)) ||
                (-(360 - ((i - 1) * rotatePerRoulette)) >= x && x >= -(360 - (i * rotatePerRoulette)))) {

                result = rMaker.choices[i - 1];
                if (result.Contains("5"))
                {
                    Random.InitState(System.DateTime.Now.Millisecond);
                    int seedNum = 20;
                    int r = Random.Range(0, seedNum);
                    if (r < count)
                    {
                        rouletteSpeed = Random.Range(12f, 13f);
                        isPlaying = true;
                        return;
                    }
                    count++;
                }
                else
                {
                    count = 1;
                }

            }
        }
        if (isIkasama && result != rMaker.choices[ikasamaID]) {
            StartCoroutine(Buttobi());
            return;
        }

        var path = UnityEngine.Application.persistentDataPath;
        var fullPath = Path.Combine(path, "result_data.csv");
        resultText.text = result + "\nが当たったよ！ " + count.ToString();
        retryButton.gameObject.SetActive(true);

        PlayerPrefs.SetInt(countKey, count);
        PlayerPrefs.Save();

        using (StreamWriter sw = new StreamWriter(fullPath, true, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            sw.WriteLine(System.DateTime.Now.ToString("yyyyMMdd\tHH:mm:ss") + "\t" + result);
        }
    }

     private IEnumerator Buttobi(){
         yield return roulette.transform.DORotate(
             new Vector3(0,0,rotatePerRoulette*((float)ikasamaID + 0.5f)),
             1.0f,
             RotateMode.FastBeyond360
         ).WaitForCompletion();
        resultText.text = rMaker.choices[ikasamaID] + "\nが当たったよ！";
        retryButton.gameObject.SetActive(true);
     }
}