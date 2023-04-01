using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System;
using Rnd = UnityEngine.Random;

public class zipZapZopScript : MonoBehaviour {

    public KMSelectable[] buttons;
    public Sprite[] hands;
    public GameObject[] pointers;
    public Sprite[] faces;
    public SpriteRenderer[] men;
    public AudioClip[] audios;

    public KMBombInfo BombInfo;
    public KMNeedyModule BombModule;
    public KMAudio audio;

    private bool active = false;
    private bool selection = false;
    private bool counting = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool _isSolved;
    int zappy = 0;
    private int lr = 0;
    private int minutes;
    private int seconds = 5;

#pragma warning disable 0649
    private bool TwitchPlaysActive;
#pragma warning restore 0649
    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i<pointers.Length; i++)
        {
            pointers[i].gameObject.SetActive(false);
        }
        if (TwitchPlaysActive) seconds = 10;
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        active = false;
        selection = false;
        foreach (KMSelectable b in buttons)
        {
            b.OnInteract += delegate () { HandlePress(b); return false; };
        }
    }
    
    protected void OnNeedyActivation()
    {
        //active = true;
        //selection = false;
        _isSolved = false;
        StartCoroutine(Zippy());
        men[0].sprite = faces[0];
        men[1].sprite = faces[0];
        
    }

    protected void OnNeedyDeactivation()
    {
        active = false;
        counting = false;
        selection = false;
        StopCoroutine(Zippy());
        
    }

    protected void OnTimerExpired()
    {
        StopCoroutine(Zippy());
        
        _isSolved = true;
        OnNeedyDeactivation();

    }
    // Update is called once per frame
    void Update () {
        if (_isSolved)
        {
            active = false;
            counting = false;
            selection = false;
        }
        minutes = (int)(BombInfo.GetTime()/60)%2;
        if (active)
        {
            StopCoroutine(Zippy());
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(true);
            if (!selection)
            {
                buttons[2].gameObject.SetActive(false);
                buttons[3].gameObject.SetActive(false);
                buttons[4].gameObject.SetActive(false);
            }
            else
            {
                buttons[2].gameObject.SetActive(true);
                buttons[3].gameObject.SetActive(true);
                buttons[4].gameObject.SetActive(true);
            }
        }
        else
        {
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(false);
            buttons[2].gameObject.SetActive(false);
            buttons[3].gameObject.SetActive(false);
            buttons[4].gameObject.SetActive(false);
        }
    }

    void HandlePress(KMSelectable button) 
    {
        Debug.LogFormat("{0}",button);
        if (button == buttons[0] || button == buttons[1])
        {
            if((button == buttons[0] && minutes == 0 && !selection)||(button == buttons[1] && minutes == 1 && !selection))
            {
                if (minutes == 0) lr = 0;
                else lr = 1;
                selection = true;
            }
            else
            {
                BombModule.HandleStrike();
                zappy = 0;
                if (button == buttons[0]) Debug.LogFormat("[Zip Zap Zop #{0}] Strike! You pressed left when the minutes remaining was an odd number.",moduleId);
                if (button == buttons[1]) Debug.LogFormat("[Zip Zap Zop #{0}] Strike! You pressed right when the minutes remaining was an even number.",moduleId);
                men[0].sprite = faces[1];
                men[1].sprite = faces[1];
                active = false;
                selection = false;
                _isSolved = true;
                BombModule.HandlePass();
                
            }
        }
        else
        {
            int j = Array.IndexOf(buttons, button);
            switch (j)
            {
                case 2:
                    audio.PlaySoundAtTransform(audios[0].ToString().Substring(0, 3), transform);
                    break;
                case 3:
                    audio.PlaySoundAtTransform(audios[1].ToString().Substring(0, 3), transform);
                    break;
                case 4:
                    audio.PlaySoundAtTransform(audios[2].ToString().Substring(0, 3), transform);
                    break;
            }
            if (!(button == buttons[5 - (3-zappy)]))
            {
                BombModule.HandleStrike();
                Debug.LogFormat("[Zip Zap Zop #{0}] Strike! You pressed {1} instead of {2}.", moduleId,button.ToString().Substring(0, 3), audios[zappy].ToString().Substring(0, 3));
                zappy = 0;
                men[0].sprite = faces[1];
                men[1].sprite = faces[1];
                BombModule.HandlePass();
                _isSolved = true;
            }
            zappy = (zappy + 1) % 3;
            active = false;
            selection = false;
            counting = false;
            StartCoroutine(Zippy());

        }
    }
    IEnumerator Zippy()
    {
        while (!active && !_isSolved)
        {
            
            yield return new WaitForSeconds(1);
            int leftRng;
            int rightRng;
            if(BombModule.CountdownTime > 5)
            {
                leftRng = Rnd.Range(1, 3);
                rightRng = Rnd.Range(1, 3);
            }
            else
            {
                leftRng = rightRng = 0;
            }
            audio.PlaySoundAtTransform(audios[zappy].ToString().Substring(0,3), transform);
            if (lr == 0)
            {
                if (leftRng == 1)
                {
                    pointers[0].gameObject.SetActive(true);
                }
                else
                {
                    pointers[2].gameObject.SetActive(true);
                    active = true;
                }
            }
            else
            {
                if (rightRng == 1)
                {
                    pointers[1].gameObject.SetActive(true);
                }
                else
                {
                    pointers[3].gameObject.SetActive(true);
                    active = true;
                }
            }
            zappy = (zappy + 1) % 3;
            lr = (lr + 1) % 2;
            yield return new WaitForSeconds(1);
            pointers[0].gameObject.SetActive(false);
            pointers[1].gameObject.SetActive(false);
            pointers[2].gameObject.SetActive(false);
            pointers[3].gameObject.SetActive(false);
            if (active)
            {
                StopCoroutine(FinalCountdown());
                StartCoroutine(FinalCountdown());
                break;
            }
        }

    }
    void Zoppy()
    {

        BombModule.HandleStrike();
        Debug.LogFormat("[Zip Zap Zop #{0}] Strike! You waited too long.", moduleId);
        men[0].sprite = faces[1];
        men[1].sprite = faces[1];
        zappy = 0;
        active = false;
        selection = false;
        counting = false;
        _isSolved = true;
        BombModule.HandlePass();

    }
    IEnumerator FinalCountdown()
    {
        counting = true;
        for(int i = 0; i < seconds*4; i++)
        {
            if (!active)
            {
                counting = false;
            }
            yield return new WaitForSeconds(0.25f);
            if (!counting) break;
        }
        if (counting) Zoppy();
        
    }
}
