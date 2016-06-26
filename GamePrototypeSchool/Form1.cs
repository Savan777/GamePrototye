using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GamePrototypeSchool
{
    public partial class Form1 : Form
    {
        //variables
        int dX; //horizontal movement, player
        int dY = -6; //verticle movement, player
        int[] bX; //horizontal movement of the blaster shots
        int[] bY; //vertical movement
        int health = 100; //health of player
        int score = 0; //score
        int jumpHeight = 0; //tracks jump height
        int jumpCap = 30; //caps jump hieght
        int movePicNum = 0; //keeps track of the pic used to animate the run
        int jumpPicNum = 0; //keeps track of the pic used to animate the jump
        int blasterNum = 0; //keeps track of the rectangle on screen
        int[] blasterPicNum; //keeps track of the pic used to animate the blaster shot
        bool invulnerable = true; //gives player invulnerable, no health loss
        bool facingRight = true; //direction player faces
        int jumpHeight2 = -180;
        int floorY;
        //=================================//
        Rectangle player; //player rectangle
        Rectangle[] floor; //rectangles for the floors
        Rectangle[] blasterShot; //rectangle for the shots/bullets
        Image playerPic; //picture for player
        Image[] blasterPic; //picture for the blaster shots
        Button startBtn; //button to start the game
        Timer screenRefresh; //refreshes screens
        Timer movementTmr; //Controls movement
        Timer animations; //timer to animate certain scenarios
        Timer invulnerableTmr; //gives time limit to invulnerable
        Random randomNum; //used for various situations

        public Form1()
        {
            //Initializing arrays
            floor = new Rectangle[8];
            blasterShot = new Rectangle[6];
            blasterPic = new Image[blasterShot.Length];
            blasterPicNum = new int[blasterShot.Length];
            bX = new int[blasterShot.Length];
            bY = new int[blasterShot.Length];
            
            for (int i = 0; i < blasterShot.Length; i++)
            {
                blasterPicNum[i] = 0;
            }

            //Creating images
            playerPic = Image.FromFile(@"PlayerRight0.png", true);

            for (int i = 0; i < blasterShot.Length; i++)
            {
                blasterPic[i] = Image.FromFile(@"BlastRight" + Convert.ToString(blasterPicNum[i]) + ".png", true);
            }

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Form settings
            this.Size = new Size(800, 500);
            this.MinimumSize = new Size(800, 500);
            this.MaximumSize = new Size(800, 500);
            this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width / 2) - (this.Width / 2), 100);

            //Rectangles
            player = new Rectangle();
            player.Size = new Size(playerPic.Width, playerPic.Height);
            player.Location = new Point((this.ClientSize.Width / 2) - (player.Width / 2), 300);

            for (int i = 0; i < floor.Length; i++)
            {
                floor[i].Size = new Size(100, 400);
                floor[i].Location = new Point((i * 100), player.Bottom);
            }
            floorY = floor[0].Y;

            for (int i = 0; i < blasterShot.Length; i++)
            {
                blasterShot[i].Size = new Size(blasterPic[0].Width, blasterPic[0].Height);
                blasterShot[i].Location = new Point(5, -100);
            }

            //Menu Buttons
            startBtn = new Button();
            startBtn.Size = new Size(100, 40);
            startBtn.Location = new Point((this.ClientSize.Width / 2) - (startBtn.Width / 2), (this.ClientSize.Height / 3));
            startBtn.FlatStyle = FlatStyle.Popup;
            startBtn.Font = new Font("Ar Essence", 12);
            startBtn.Text = "Begin!";
            startBtn.Visible = true;
            startBtn.Click += startBtn_Click;

            //Creating timers
            screenRefresh = new Timer();
            screenRefresh.Interval = 10;
            screenRefresh.Tick += screenRefresh_Tick;

            movementTmr = new Timer();
            movementTmr.Interval = 10;
            movementTmr.Tick += movementTmr_Tick;

            animations = new Timer();
            animations.Interval = 40;
            animations.Tick += animations_Tick;

            invulnerableTmr = new Timer();
            invulnerableTmr.Interval = 5000;
            invulnerableTmr.Tick += invulnerableTmr_Tick;

            //Key events
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            this.DoubleBuffered = true;

            menu(); //brings up the menu
        }

        void Form1_Paint(object sender, PaintEventArgs e)
        { //draws the player and floor
            e.Graphics.DrawImage(playerPic, player);

            for (int i = 0; i < floor.Length; i++)
            {
                e.Graphics.FillRectangles(Brushes.Black, floor);
            }

            for (int i = 0; i < blasterShot.Length; i++)
            {
                e.Graphics.DrawImage(blasterPic[i], blasterShot[i]);
            }
        }

        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            { //When either left or right arrow is let go stop movement
                dX = 0;
            }
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && dX <= 0)
            { //on pressing Left the background will move right
                dX = 4;
                facingRight = false;
                movePicNum = 0; //reset so changing directions starts the run animation at the first picture
            }
            else if (e.KeyCode == Keys.Right && dX >= 0)
            { //on pressing Right the background will move left
                dX = -4;
                facingRight = true;
                movePicNum = 0;
            }

            if (e.KeyCode == Keys.Up && isPlayerOnGround() && jumpHeight < jumpCap)//← last and is for rollback jump code
            { //on pressing Up background will move down
                dY = 6;
            }

            if (e.KeyCode == Keys.Space && blasterShot[blasterNum].Y < 0 && facingRight)
            {
                blasterShot[blasterNum].Y = (player.Y + 4);
                blasterShot[blasterNum].X = player.Right;
                bX[blasterNum] = 6;

                if (blasterNum < blasterShot.Length - 1)
                {
                    blasterNum++;
                }
            }
            else if (e.KeyCode == Keys.Space && blasterShot[blasterNum].Y < 0 && !facingRight)
            {
                blasterShot[blasterNum].Y = (player.Y + 4);
                blasterShot[blasterNum].X = (player.Left - blasterShot[blasterNum].Width);
                bX[blasterNum] = -6;

                if (blasterNum < (blasterShot.Length - 1))
                {
                    blasterNum++;
                }
            }
        }

        void startBtn_Click(object sender, EventArgs e)
        {
            //create the paint event
            this.Paint += Form1_Paint;

            //gets rid of the menu
            this.Controls.Remove(startBtn);

            //start timers
            screenRefresh.Start();
            movementTmr.Start();
            animations.Start();
            invulnerableTmr.Start();
        }

        void screenRefresh_Tick(object sender, EventArgs e)
        { //this timers sole purpose is to refresh the screen, other miscellaneous stuff may be here
            this.Refresh();
        }

        void movementTmr_Tick(object sender, EventArgs e)
        { //this timer will move the enviroment, the player will stay in one spot
            //but will look as if it is moving
            for (int i = 0; i < floor.Length; i++)
            { //moves the floor in a horizontal direction
                floor[i].X += dX;

                floorOffScreen();
            }

            if (false)
            {
                if (!isPlayerOnGround() && dY < 0)
                { //if this method returns false player is in the air
                    for (int i = 0; i < floor.Length; i++)
                    { //this requires its own for loop to move all rectangles at one time
                        floor[i].Y += dY;
                    }
                }
                else if (dY > 0 && jumpHeight < jumpCap)
                { //this will run if the height of player is less than the max height
                    for (int i = 0; i < floor.Length; i++)
                    {
                        floor[i].Y += dY;
                    }

                    jumpHeight++; //track how many times the player has 'moved' vertically
                    if (jumpHeight >= jumpCap)
                    { //when max jump height is reached change dY to move floor up, player down
                        dY = -6;
                    }
                }
            }

            //====================================
            if (!isPlayerOnGround() && dY < 0)
            { //if this method returns false player is in the air
                for (int i = 0; i < floor.Length; i++)
                { //this requires its own for loop to move all rectangles at one time
                    floor[i].Y += dY;
                }
                floorY = floor[0].Y; //this is used to get the y value of the floor
                //depending on the level the player will start in the air, this makes sure the player falls to meet the floor

                for (int i = 0; i < blasterShot.Length; i++)
                {
                    if (blasterShot[i].Bottom >= 0)
                    {
                        if (!isPlayerOnGround() && dY < 0)
                        {
                            bY[i] = -6;
                        }

                        blasterShot[i].Y += bY[i];

                        if (blasterShot[i].Bottom < 0 || blasterShot[i].Y > this.ClientSize.Height)
                        {
                            blasterShot[i].Location = new Point(5, -100);

                            if (blasterNum == (blasterShot.Length - 1))
                            {
                                blasterNum = 0;
                            }
                        }
                    }
                }
            }
            else if (dY > 0)
            { //the value of dy will be greater than zero on jumping
                for (int i = 0; i < floor.Length; i++)
                { //moves all of the floors acording to this equation
                    floor[i].Y = -1 * (Math.Abs(jumpHeight2) - (180 + floorY));

                    if (i < blasterShot.Length && jumpHeight2 < 0)
                    {
                        bY[i] = 6;
                    }
                    else if (i < blasterShot.Length && jumpHeight2 > 0)
                    {
                        bY[i] = -6;
                    }
                }

                if (jumpHeight2 < 180)
                { //increases jumpheight by an amount if less than the absolute value of the original amount
                    jumpHeight2 += 6;
                }
                else
                { //sets jump height to its original amount and sets dy to -6
                    jumpHeight2 = -180;
                    dY = -6;
                    
                    for (int i = 0; i < blasterShot.Length; i++)
                    {
                        bY[i] = 0;
                    }
                }
            }

            if (!isPlayerOnGround() && dY < 6 && jumpPicNum == 9)
            { //falling off a platform stops the jump animation half way this fixes it
                //by setting the value of jumpheight past halfway
                jumpHeight2 = 6;
            }
            //==================================

            if (isPlayerOnGround())
            { //if player hits the ground allow player to jump
                jumpHeight = 0;
            }

            //Blaster shot movement
            for (int i = 0; i < blasterShot.Length; i++)
            {
                if (blasterShot[i].Bottom > 0)
                {
                    blasterShot[i].X += bX[i];
                    blasterShot[i].Y += bY[i];

                    if (blasterShot[i].Right < 0 || blasterShot[i].X > this.ClientSize.Width)
                    {
                        blasterShot[i].Location = new Point(5, -100);
                        
                        if (blasterNum == (blasterShot.Length - 1))
                        {
                            blasterNum = 0;
                        }
                    }
                }
            }
        }

        void animations_Tick(object sender, EventArgs e)
        { //animates various situations
            //Animate run
            if (isPlayerOnGround() && dX < 0)
            { //will only run when player is on ground
                if (movePicNum > 13)
                { //max of 13 pictures for the run animation
                    movePicNum = 0;
                }

                playerPic = Image.FromFile(@"PlayerRunRight" + Convert.ToString(movePicNum) + ".png", true);
                player.Size = new Size(playerPic.Width, playerPic.Height);

                movePicNum++;
            }
            else if (isPlayerOnGround() && dX > 0)
            { //running in a different direction
                if (movePicNum > 13)
                {
                    movePicNum = 0;
                }

                playerPic = Image.FromFile(@"PlayerRunLeft" + Convert.ToString(movePicNum) + ".png", true);
                player.Size = new Size(playerPic.Width, playerPic.Height);

                movePicNum++;
            }
            else if (isPlayerOnGround() && facingRight)
            { //these two set the picture when staying still
                playerPic = Image.FromFile(@"PlayerRight0.png", true);
                player.Size = new Size(playerPic.Width, playerPic.Height);
            }
            else if (isPlayerOnGround() && !facingRight)
            {
                playerPic = Image.FromFile(@"PlayerLeft0.png", true);
                player.Size = new Size(playerPic.Width, playerPic.Height);
            }

            //Animate jump (works with absolute jump)
            if (jumpPicNum > 18 && isPlayerOnGround())
            { //max of 18 jump pictures
                jumpPicNum = 0;
            }

            if (!isPlayerOnGround() && facingRight)
            {
                if (jumpPicNum <= 18)
                {
                    playerPic = Image.FromFile(@"PlayerJumpRight" + Convert.ToString(jumpPicNum) + ".png", true);
                }
                else if (jumpPicNum > 18)
                { //if jumpPicNum has reached the end and direction changed from Left to Right fix the picture
                    playerPic = Image.FromFile(@"PlayerJumpRight18.png", true);
                }

                if (jumpHeight2 <= 0 && jumpPicNum < 9)
                { //stops the jump animation halfway if max height is not yet reached
                    jumpPicNum++;
                    player.Size = new Size(playerPic.Width, playerPic.Height);
                }
                else if (jumpHeight2 > 0 && jumpPicNum >= 9)
                { //resumes the jump animation after player starts falling down
                    jumpPicNum++;
                    player.Size = new Size(playerPic.Width, playerPic.Height);
                }
            }
            else if (!isPlayerOnGround() && !facingRight)
            {
                if (jumpPicNum <= 18)
                {
                    playerPic = Image.FromFile(@"PlayerJumpLeft" + Convert.ToString(jumpPicNum) + ".png", true);
                }
                else if (jumpPicNum > 18)
                { //if jumpPicNum has reached the end and direction changed from Right to Left fix the picture
                    playerPic = Image.FromFile(@"PlayerJumpLeft18.png", true);
                }

                if (jumpHeight2 <= 0 && jumpPicNum < 9)
                {
                    jumpPicNum++;
                    player.Size = new Size(playerPic.Width, playerPic.Height);
                }
                else if (jumpHeight2 > 0 && jumpPicNum >= 9)
                {
                    jumpPicNum++;
                    player.Size = new Size(playerPic.Width, playerPic.Height);
                }
            }
        }

        void invulnerableTmr_Tick(object sender, EventArgs e)
        { //when this ticks the player is no longer invulnerable, timer stops itself
            invulnerable = false;
            invulnerableTmr.Stop();
        }

        public void menu()
        { //adds all the menu stuff to the form
            this.Controls.Add(startBtn);
        }

        private bool isPlayerOnGround()
        { //finds out whether the player reaches the floor, only the top bit of the floor counts
            for (int i = 0; i < floor.Length; i++)
            {
                if (player.IntersectsWith(floor[i]) && player.Bottom <= (floor[i].Top + 10))
                { //if player lands on the floor return true
                    return true;
                }
            }
            //if player does not intersect with any floor return false
            return false;
        }

        private void playerHit()
        { //checks to see if the player hit an enemy
            if (!invulnerable)
            {

                invulnerable = true;
                invulnerableTmr.Start();
            }
        }

        private void floorOffScreen()
        { //currently NOT a set level, used to alow 'infinite' movement in one direction
            for (int i = 0; i < floor.Length; i++)
            {
                if (dX > 0 && floor[i].Left >= this.ClientSize.Width)
                {
                    floor[i].X = (0 - floor[i].Width);
                }
                else if (dX < 0 && floor[i].Right <= 0)
                {
                    floor[i].X = this.ClientSize.Width;
                }
            }
        }
    }
}