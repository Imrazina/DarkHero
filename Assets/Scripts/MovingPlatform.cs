using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Vector2 target;
    private Rigidbody2D rb;
    private Vector2 velocity; // ⬅️ сохраняем скорость платформы

    public Vector2 PlatformVelocity => velocity; // геттер для игроков

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.position = pointA.position;
        target = (Vector2)pointB.position;
    }

    void FixedUpdate()
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        velocity = (newPos - rb.position) / Time.fixedDeltaTime; // ⬅️ считаем скорость
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, target) < 0.05f)
        {
            target = (target == (Vector2)pointA.position) ? (Vector2)pointB.position : (Vector2)pointA.position;
        }
    }
}