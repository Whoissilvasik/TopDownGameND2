using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour, ICloneable, IEnumerable<string>
{
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 10f;
    private Vector2 inputVector;
    private Rigidbody2D rb;
    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    private Inventory<string> inventory = new Inventory<string>();

    // if player attacking
    public event EventHandler OnPlayerAttack;

    private void Awake()
    {
        try
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
        }
        catch (Exception ex)
        {
            Debug.LogError("Not initialized Player: " + ex.Message);
        }
    }

    private void Start()
    {
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
        OnPlayerAttack += Player_OnPlayerAttack;

        // Add to inventory
        inventory.Add("Sword");
        inventory.Add("Shield");

        // What we have ?
        foreach (var item in this)  // IEnumerable<string>
        {
            Debug.Log("Item in inventory: " + item);
        }
    }

    private void Update()
    {
        inputVector = GameInput.Instance.GetMovementVector();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }

    private void GameInput_OnPlayerAttack(object sender, EventArgs e)
    {
        try
        {
            ActiveWeapon.Instance.GetActiveWeapon().Attack();
        }
        catch (PlayerException ex)
        {
            Debug.LogError("Attack is not working change it!!!!!!!!!: " + ex.Message);
        }
    }

    private void Player_OnPlayerAttack(object sender, EventArgs e)
    {
        Debug.Log("Attacking!");
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    // Реализация интерфейса IEnumerable<string>
    public IEnumerator<string> GetEnumerator()
    {
        foreach (var item in inventory.Items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        return Camera.main.WorldToScreenPoint(transform.position);
    }
}

public class PlayerException : Exception
{
    public PlayerException(string message) : base(message) { }
}

public class Inventory<T>
{
    private List<T> items = new List<T>();

    public IEnumerable<T> Items => items;

    public void Add(T item)
    {
        items.Add(item);
    }

    public void Remove(T item)
    {
        if (!items.Remove(item))
        {
            throw new PlayerException("You haven't got it in inventory!");
        }
    }
}

public static class VectorExtensions
{
    public static Vector3 To2D(this Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, 0);
    }
}