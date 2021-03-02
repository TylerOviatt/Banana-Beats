using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum SliderType
{
    LeftSlider,
    RightSlider
}

//interface shared between RegularSliders and HoldSliders
public interface SliderInterface
{
    SliderType sliderType { get; set; }

    float horizontal_speed { get; set; }

    bool can_destroy { get; set; }

    void Darken();

    void DetectHit();
}


public class RegularSlider : MonoBehaviour, SliderInterface
{

    [HideInInspector]
    public bool can_destroy { get; set; } = true;
    public float horizontal_speed { get; set; }
    public SliderType sliderType { get { return _sliderType; } set { _sliderType = value; } }
    [SerializeField]
    private SliderType _sliderType;
    public Vector3 direction = Vector3.right;
    public SpriteRenderer spriteRenderer;
    public KeyCode clickKey;

    public void Darken()
    {
        Color tempColor = Color.black;
        tempColor.a = 0.4f;
        spriteRenderer.color = tempColor;
    }


    HitScore hitScore;

    void HitDetected() {
        Destroy(this.gameObject);
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

            HitDetected();
        }

    }

    void Update()
    {
        this.transform.position += (direction * horizontal_speed * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == GlobalHelper.perfectRegionLayer)
        {
            hitScore = HitScore.Perfect;

        }
        else if (collision.gameObject.layer == GlobalHelper.goodRegionLayer)
        {

            hitScore = HitScore.Good;

        }
    }

}