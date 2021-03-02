using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//the hold slider is a start slider and an end slider with a body. the player holds down the key when interacting with this slider
public class HoldSlider : MonoBehaviour, SliderInterface
{

    [HideInInspector]
    public bool can_destroy { get; set; } = true;
    public float length = 10f;
    public EndSliderCompletion endSliderCompletion;
    public Transform body;
    public SpriteRenderer bodySprite;
    public Transform endSlider;
    public SpriteRenderer startSliderSpriteRenderer;
    public SpriteRenderer endSliderSpriteRenderer;
    public Transform start;
    public GameObject spriteMaskRoot;
    public float horizontal_speed { get; set; }
    public EndSliderFailBox endSliderFailBoxScript;
    public SliderType sliderType { get { return _sliderType; } set { _sliderType = value; } }
    [SerializeField]
    private SliderType _sliderType;
    HitScore hitScore;
    public Vector3 direction = Vector3.left;
    public KeyCode clickKey;
    public float scoreModifier = 0.1f;

    public void Initialize(float length)
    {
        if (_sliderType == SliderType.RightSlider)
        {
            endSlider.position = new Vector3(start.transform.position.x + length, endSlider.position.y, 0f);
            body.localScale = new Vector3((endSlider.localPosition.x / 10f), body.localScale.y, 0f);
        }
        else if (_sliderType == SliderType.LeftSlider)
        {

            endSlider.position = new Vector3(start.transform.position.x - length, endSlider.position.y, 0f);
            body.localScale = new Vector3(-(endSlider.localPosition.x / 10f), body.localScale.y, 0f);
        }


        endSliderCompletion.endSlider = endSlider.gameObject;
    }

    void Update()
    {
        start.transform.position += (direction * horizontal_speed * Time.deltaTime);
    }

    public void DetectHit() {

        if (Input.GetKeyDown(clickKey))
        {

            if (hitScore == HitScore.Perfect)
            {
                GlobalHelper.global.scoreManager.score += 10;
                GlobalHelper.global.scoreManager.combo += 1;
                GlobalHelper.global.hitScoreText.text = "PERFECT";
                GlobalHelper.global.smileys.ActivateSmiley(Smiley.Happy);

            }
            else if (hitScore == HitScore.Good)
            {
                GlobalHelper.global.scoreManager.score += 5;
                GlobalHelper.global.scoreManager.combo += 1;
                GlobalHelper.global.hitScoreText.text = "GOOD";
                GlobalHelper.global.smileys.ActivateSmiley(Smiley.Happy);
            }

            StartCoroutine(HoldDown());

        }

    }
    bool firstHold = false;
    IEnumerator HoldDown()
    {

        if (firstHold) yield break;
        firstHold = true;
        can_destroy = false;
        //instantiates the sprite mask gameobject and sets it to the parent of this gameobject so that it won't mask any other sprites outside
        GameObject spriteMaskRootInstance = Instantiate(spriteMaskRoot);
        spriteMaskRootInstance.transform.position = this.transform.position;
        this.transform.parent = spriteMaskRootInstance.transform;
        //set it to this transform's x position
        spriteMaskRoot.transform.position = new Vector3(this.transform.position.x, spriteMaskRoot.transform.position.y, spriteMaskRoot.transform.position.z);

        //removes the child that contains the box collider so we can detect the end slider later
        if (endSliderCompletion != null)
            endSliderCompletion.gameObject.transform.parent = null;


        //deparent the fail box so it stays stationary and is ready to catch the end slider
        endSliderFailBoxScript.endSlider = endSlider.gameObject;
        endSliderFailBoxScript.transform.SetParent(null);

        while (true)
        {
            GlobalHelper.global.scoreManager.score += scoreModifier * Time.deltaTime;

            //detects if the player lets go of a hold in order to "catch" the end slider
            if (Input.GetKeyUp(clickKey))
            {
                //detach from the sprite mask root since we need the sprite mask to move with the slider now
                this.transform.parent = null;
                //makes sure the sprite mask only works on children of the hold slider; SortinGroup component needs to be added to the root parent for this to function
                SortingGroup sortingGroupInstance = gameObject.AddComponent(typeof(SortingGroup)) as SortingGroup;

                if (sliderType == SliderType.RightSlider) {
                    sortingGroupInstance.sortingLayerName = "RightSlider";
                } else {
                    sortingGroupInstance.sortingLayerName = "LeftSlider";
                }


                //parent the sprite mask to the slider so it moves with the slider
                Transform spriteMaskChild = spriteMaskRootInstance.transform.GetChild(0);
                spriteMaskChild.SetParent(this.transform);

                //destroy the sprite mask's previous root since we don't need it anymore
                Destroy(spriteMaskRootInstance);


                if (endSliderCompletion.DetectHit())
                {
                    GlobalHelper.global.smileys.ActivateSmiley(Smiley.Happy);
                    GlobalHelper.global.hitScoreText.text = "PERFECT";
                    GlobalHelper.global.scoreManager.score += 5;
                    GlobalHelper.global.scoreManager.combo += 1;
                    Destroy(this.gameObject);
                }
                else
                {
                   
                    GlobalHelper.global.hitScoreText.text = "MISS";
                    GlobalHelper.global.smileys.ActivateSmiley(Smiley.Meh);
                    GlobalHelper.global.scoreManager.combo = 0;
                }

                Destroy(endSliderFailBoxScript.gameObject);
                Destroy(endSliderCompletion.gameObject);

                Darken();

                break;
            }
            yield return null;
        }
    }

    //triggers whenever the end slider enters the fail box
    public void TriggerFail()
    {

        if(endSliderFailBoxScript != null) Destroy(endSliderFailBoxScript.gameObject);
        if(endSliderCompletion.gameObject != null) Destroy(endSliderCompletion.gameObject);
        GlobalHelper.global.hitScoreText.text = "FAIL";
        GlobalHelper.global.scoreManager.combo = 0;
        GlobalHelper.global.smileys.ActivateSmiley(Smiley.Angry);
        Destroy(this.gameObject);

    }


    public void Darken()
    {
        float tempAlpha = bodySprite.color.a;
        Color tempColor = Color.black;
        tempColor.a = tempAlpha;

        bodySprite.color = tempColor;

        tempColor = Color.black;
        tempColor.a = 0.6f;


        endSliderSpriteRenderer.color = tempColor;

        startSliderSpriteRenderer.color = tempColor;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            hitScore = HitScore.Perfect;
        }
        else if (collision.gameObject.layer == 11)
        {
            hitScore = HitScore.Good;
        }
    }


}