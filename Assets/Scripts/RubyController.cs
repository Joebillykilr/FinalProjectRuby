using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;
    public float timeInvincible = 2.0f;
    public GameObject projectilePrefab;

    public int score;
    public Text scoreText;

    public bool gameOver = false;
    public static bool staticVar = false;

    public GameObject player;

    public Text winText;

    public AudioClip throwSound;
    public AudioClip hitSound;

    public AudioClip talkSound;
    public AudioClip speedSound;

    public ParticleSystem damageEffect;
    public ParticleSystem healEffect;

    public int health { get { return currentHealth; }}
    int currentHealth;

    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal; 
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    public AudioSource audioSource;

    public AudioClip musicClipOne;
    public AudioClip musicClipTwo;
    public AudioClip mainMusic;

    public int cogs;
    public Text CogsText;

    public int skull;
    public Text SkullText;

    private bool HasAmmo = true;

    //public bool isStage2 = false;
    //public bool gotoStage2;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        rigidbody2d = GetComponent<Rigidbody2D>();
        score = 0;
        skull = 0;
        cogs = 4;
        SetScoreText ();
        winText.text = "";
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("RubyController");
        audioSource.clip = mainMusic;
        audioSource.Play();
        SetCogsText ();
        SetSkullText ();
    }

    // Update is called once per frame
  void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }
        
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if(Input.GetKeyDown(KeyCode.C) && HasAmmo)
        {
            cogs = cogs - 1;
            Launch();
            SetCogsText();
        }

        //if (health == 0)
        //{
            //audioSource.clip = musicClipTwo;
            //audioSource.Play();
        //}

        if(health == 0)
        {
            winText.text = "You lost! Press R to restart";
            //audioSource.clip = musicClipTwo;
            //audioSource.Play();
            Time.timeScale = 0;

            if (Input.GetKey(KeyCode.R))
            {
                gameOver = true;
            }
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (score == 4)
        {
            if (Input.GetKey(KeyCode.R))
            {
                gameOver = true;
            }
        }
    
        if (Input.GetKeyDown(KeyCode.X))
        {
        RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
           if (hit.collider != null)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name == "MainScene" && score == 4)
                {
                    SceneManager.LoadScene("MainScene 1");
                }
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                    PlaySound(talkSound);
                }
            }
        }

        if (gameOver == true)
        {

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene
            Time.timeScale = 1;
        }
        
    }


    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            damageEffect.Play();
            PlaySound(hitSound);
        }

        if (amount > 0)
        {
            healEffect.Play();
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        if (currentHealth == 0)
        {
            audioSource.clip = musicClipTwo;
            audioSource.Play();
        }
    }

    public void ChangeScore(int scoreAmount)
    {
        score = score + 1;
        SetScoreText ();
    }

    void SetScoreText ()
    {
        scoreText.text = "Robots Fixed: " + score.ToString ();
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "MainScene 1")
        {
            if (score == 4 && skull == 1)
            {
                winText.text = "You Win! Game created by Alexander Shopovick, Press R to restart";
                audioSource.clip = musicClipOne;
                audioSource.Play();
                Time.timeScale = 0;
        
                //if (Input.GetKey(KeyCode.R))
                //{
                //    gameOver = true;
                //}

            }
        }
    }

    void SetCogsText ()
    {
        CogsText.text = "Cogs: " + cogs.ToString ();
        if (cogs <= 0)
        {
            HasAmmo = false;
        }
        else
        {
            HasAmmo = true;
        }
    }

    void SetSkullText ()
    {
        SkullText.text = "Skull: " + skull.ToString ();
        SetScoreText ();
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == ("Cog"))
        {
            {
                cogs = cogs + 3;
                Destroy(collision.collider.gameObject);
                SetCogsText ();
            }
        }

        if (collision.collider.tag == ("Skull"))
        {
            skull = skull + 1;
            Destroy(collision.collider.gameObject);
            SetSkullText ();
        }

        if (collision.collider.tag == ("Speed"))
        {
            speed = 4;
            Destroy(collision.collider.gameObject);
            PlaySound(speedSound);
        }
    }
}
