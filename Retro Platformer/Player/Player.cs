using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    private float spd;
    [Export]
    private float acc;
    [Export]
    private float frc;
    [Export]
    private float dec;
    [Export]
    private float grv;
    [Export]
    private float maxGrv;
    [Export]
    private float minGrv;
    [Export]
    private float slp = 30;
    [Export] private float jumpHeight = 650;
    [Export] private float minJump = 150;
    private Vector2 velocity;

    private float rotSpd = 0.65f;
    private float correctionSpd = 0.875f;

    private float rot = 0;
    private float gsp = 0;
    private float vsp = 0;
    private float rotAngle = 0;

    private RayCast2D sensorL;
    private RayCast2D sensorR;
    private RayCast2D sensorC;
    private Sprite sprite;

    private Vector2 floorNormal = new Vector2(0, -1);

    public override void _Ready() 
    {
        sprite = (Sprite)GetNode("Sprite");
        sensorL = (RayCast2D)GetNode("LSensor");
        sensorR =  (RayCast2D)GetNode("RSensor");
        sensorC = (RayCast2D)GetNode("CSensor");
    }

    public override void _Process(float delta) 
    {
        floorNormal = GetFloorNormal();
        rot = -floorNormal.AngleTo(new Vector2(0,-1));
        //sensorC.Rotation = rot;
        if (sensorC.IsColliding()) sprite.Rotation = Mathf.Lerp(sprite.Rotation, -sensorC.GetCollisionNormal().AngleTo(new Vector2(0,-1)), rotSpd);
        else sprite.Rotation = Mathf.Lerp(sprite.Rotation, rot, rotSpd);
        if (sensorC.IsColliding()) sprite.GlobalPosition = sensorC.GetCollisionPoint() - (new Vector2(0, 30));
        //if (sensorL.IsColliding() || sensorR.IsColliding()) sprite.GlobalPosition = GetCollisionPoint() - new Vector2(0, 30);
        else sprite.Position = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta) 
    {
        velocity = Vector2.Zero;
    
        MoveAndSlide(new Vector2(0, minGrv).Rotated(rot), floorNormal);
        if (sensorL.IsColliding() || sensorR.IsColliding()) GroundMove();
        else AirMove();

        // Handle vertical movement
        if (IsOnFloor()) 
        {
            rotAngle = rot;
            vsp = 0;
            if (Input.IsActionJustPressed("jump")) 
            {
                vsp = -jumpHeight;
            }

        }
        else 
        {
            // Gravity
            vsp += grv;
            if (vsp > maxGrv) vsp = maxGrv;
            if (Input.IsActionJustPressed("jump") && (sensorL.IsColliding() || sensorR.IsColliding())) 
            {
                vsp = -jumpHeight;
            }

            // Releasing jump button
            if (Input.IsActionJustReleased("jump") && vsp < -minJump) 
            {
                vsp = -minJump;
            }

            // Ceiling detection
            if (IsOnCeiling() && vsp < 0) vsp = 0;
        }
        // Handle slopes
        if (vsp < 0) velocity = new Vector2(0, vsp).Rotated(rotAngle);
        else velocity = new Vector2(0, vsp);
        if (rot == 0) velocity.x += gsp;
        else 
        {
            if (Mathf.Sign(rot) != Mathf.Sign(gsp)) velocity.x += gsp;
            else velocity += new Vector2(gsp, 0).Rotated(rot);
        }

        GD.Print(velocity);
        // Apply movement
        MoveAndSlide(velocity, floorNormal);
    }

    public Vector2 GetFloorNormal() 
    {
        if (sensorL.IsColliding() && sensorR.IsColliding()) 
        {
            if (sensorL.GetCollisionPoint().y >= sensorR.GetCollisionPoint().y) return sensorL.GetCollisionNormal();
            else return sensorR.GetCollisionNormal();
        }
        else if (sensorL.IsColliding()) return sensorL.GetCollisionNormal();
        else if (sensorR.IsColliding()) return sensorR.GetCollisionNormal();
        else return new Vector2(0, -1);
    }

    public Vector2 GetCollisionPoint()
    {
        if (sensorL.IsColliding() && sensorR.IsColliding()) 
        {
            if (sensorL.GetCollisionPoint().y >= sensorR.GetCollisionPoint().y) return sensorL.GetCollisionPoint() + new Vector2(14.2f, 0).Rotated(rot);
            else return sensorR.GetCollisionPoint() - new Vector2(14.2f, 0).Rotated(rot);
        }
        else if (sensorL.IsColliding()) return sensorL.GetCollisionPoint() + new Vector2(14.2f, 0).Rotated(rot);
        else if (sensorR.IsColliding()) return sensorR.GetCollisionPoint() - new Vector2(14.2f, 0).Rotated(rot);
        else return new Vector2(0, 0);
    }

    public void GroundMove() 
    {
        gsp += slp * Mathf.Sin(rot);
        if (Input.IsActionPressed("move_left")) 
        {
        if (gsp > 0) gsp -= dec;
            else gsp -= acc;
        }
        else if (Input.IsActionPressed("move_right")) 
        {
            if (gsp < 0) gsp += dec;
            else gsp += acc;
        }
        else 
        {
            if (Mathf.Abs(gsp) < frc) gsp = 0;
            else gsp -= Mathf.Sign(gsp) * frc;
        }
        if (Mathf.Abs(gsp) > spd) gsp = spd * Mathf.Sign(gsp);
    }

    public void AirMove() 
    {
        GD.Print("ho");
        if (Input.IsActionPressed("move_left")) 
        {
            gsp -= acc * 2;
        }
        else if (Input.IsActionPressed("move_right")) 
        {
            gsp += acc * 2;
        }
        else 
        {
            if (vsp < 0 && vsp > -500)
            {
                gsp *= 0.7658f;
            }                
        }
        if (Mathf.Abs(gsp) > spd) gsp = spd * Mathf.Sign(gsp);  
    }
}
