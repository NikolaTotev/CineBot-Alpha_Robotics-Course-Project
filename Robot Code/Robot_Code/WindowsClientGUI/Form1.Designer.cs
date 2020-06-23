namespace WindowsClientGUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Btn_Connect = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Btn_PathFollow = new System.Windows.Forms.Button();
            this.Btn_ObjectTracking = new System.Windows.Forms.Button();
            this.Btn_GimbalJog = new System.Windows.Forms.Button();
            this.Btn_TestMode = new System.Windows.Forms.Button();
            this.Btn_StepperJog = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Lb_ClientStarted = new System.Windows.Forms.Label();
            this.Lb_ClientConnected = new System.Windows.Forms.Label();
            this.Lb_ConnectionRetry = new System.Windows.Forms.Label();
            this.Lb_CommandSent = new System.Windows.Forms.Label();
            this.Lb_AwaitingResponse = new System.Windows.Forms.Label();
            this.Lb_ClientError = new System.Windows.Forms.Label();
            this.Lb_AbortedConnection = new System.Windows.Forms.Label();
            this.Lb_WaitingForCommand = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Btn_Connect
            // 
            this.Btn_Connect.Location = new System.Drawing.Point(15, 32);
            this.Btn_Connect.Name = "Btn_Connect";
            this.Btn_Connect.Size = new System.Drawing.Size(178, 43);
            this.Btn_Connect.TabIndex = 0;
            this.Btn_Connect.Text = "Connect";
            this.Btn_Connect.UseVisualStyleBackColor = true;
            this.Btn_Connect.Click += new System.EventHandler(this.Btn_Connect_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.PeachPuff;
            this.panel1.Controls.Add(this.Btn_PathFollow);
            this.panel1.Controls.Add(this.Btn_ObjectTracking);
            this.panel1.Controls.Add(this.Btn_GimbalJog);
            this.panel1.Controls.Add(this.Btn_TestMode);
            this.panel1.Controls.Add(this.Btn_StepperJog);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.Btn_Connect);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(205, 328);
            this.panel1.TabIndex = 1;
            // 
            // Btn_PathFollow
            // 
            this.Btn_PathFollow.Location = new System.Drawing.Point(15, 228);
            this.Btn_PathFollow.Name = "Btn_PathFollow";
            this.Btn_PathFollow.Size = new System.Drawing.Size(178, 43);
            this.Btn_PathFollow.TabIndex = 6;
            this.Btn_PathFollow.Text = "Path Follow";
            this.Btn_PathFollow.UseVisualStyleBackColor = true;
            this.Btn_PathFollow.Click += new System.EventHandler(this.Btn_PathFollow_Click);
            // 
            // Btn_ObjectTracking
            // 
            this.Btn_ObjectTracking.Location = new System.Drawing.Point(15, 179);
            this.Btn_ObjectTracking.Name = "Btn_ObjectTracking";
            this.Btn_ObjectTracking.Size = new System.Drawing.Size(178, 43);
            this.Btn_ObjectTracking.TabIndex = 5;
            this.Btn_ObjectTracking.Text = "Object Tracking";
            this.Btn_ObjectTracking.UseVisualStyleBackColor = true;
            this.Btn_ObjectTracking.Click += new System.EventHandler(this.Btn_ObjTracking_Click);
            // 
            // Btn_GimbalJog
            // 
            this.Btn_GimbalJog.Location = new System.Drawing.Point(15, 130);
            this.Btn_GimbalJog.Name = "Btn_GimbalJog";
            this.Btn_GimbalJog.Size = new System.Drawing.Size(178, 43);
            this.Btn_GimbalJog.TabIndex = 4;
            this.Btn_GimbalJog.Text = "Gimbal Jog";
            this.Btn_GimbalJog.UseVisualStyleBackColor = true;
            this.Btn_GimbalJog.Click += new System.EventHandler(this.Btn_GimbalJog_Click);
            // 
            // Btn_TestMode
            // 
            this.Btn_TestMode.Location = new System.Drawing.Point(15, 277);
            this.Btn_TestMode.Name = "Btn_TestMode";
            this.Btn_TestMode.Size = new System.Drawing.Size(178, 43);
            this.Btn_TestMode.TabIndex = 3;
            this.Btn_TestMode.Text = "Test Mode";
            this.Btn_TestMode.UseVisualStyleBackColor = true;
            this.Btn_TestMode.Click += new System.EventHandler(this.Btn_TestMode_Click);
            // 
            // Btn_StepperJog
            // 
            this.Btn_StepperJog.Location = new System.Drawing.Point(15, 81);
            this.Btn_StepperJog.Name = "Btn_StepperJog";
            this.Btn_StepperJog.Size = new System.Drawing.Size(178, 43);
            this.Btn_StepperJog.TabIndex = 2;
            this.Btn_StepperJog.Text = "Stepper Jog";
            this.Btn_StepperJog.UseVisualStyleBackColor = true;
            this.Btn_StepperJog.Click += new System.EventHandler(this.Btn_StepperJog_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Control Panel";
            // 
            // Lb_ClientStarted
            // 
            this.Lb_ClientStarted.AutoSize = true;
            this.Lb_ClientStarted.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_ClientStarted.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_ClientStarted.Location = new System.Drawing.Point(12, 356);
            this.Lb_ClientStarted.Name = "Lb_ClientStarted";
            this.Lb_ClientStarted.Size = new System.Drawing.Size(113, 21);
            this.Lb_ClientStarted.TabIndex = 7;
            this.Lb_ClientStarted.Text = "Client Started";
            // 
            // Lb_ClientConnected
            // 
            this.Lb_ClientConnected.AutoSize = true;
            this.Lb_ClientConnected.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_ClientConnected.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_ClientConnected.Location = new System.Drawing.Point(12, 386);
            this.Lb_ClientConnected.Name = "Lb_ClientConnected";
            this.Lb_ClientConnected.Size = new System.Drawing.Size(140, 21);
            this.Lb_ClientConnected.TabIndex = 8;
            this.Lb_ClientConnected.Text = "Client Connected";
            // 
            // Lb_ConnectionRetry
            // 
            this.Lb_ConnectionRetry.AutoSize = true;
            this.Lb_ConnectionRetry.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_ConnectionRetry.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_ConnectionRetry.Location = new System.Drawing.Point(12, 417);
            this.Lb_ConnectionRetry.Name = "Lb_ConnectionRetry";
            this.Lb_ConnectionRetry.Size = new System.Drawing.Size(140, 21);
            this.Lb_ConnectionRetry.TabIndex = 9;
            this.Lb_ConnectionRetry.Text = "Connection Retry";
            // 
            // Lb_CommandSent
            // 
            this.Lb_CommandSent.AutoSize = true;
            this.Lb_CommandSent.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_CommandSent.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_CommandSent.Location = new System.Drawing.Point(12, 448);
            this.Lb_CommandSent.Name = "Lb_CommandSent";
            this.Lb_CommandSent.Size = new System.Drawing.Size(129, 21);
            this.Lb_CommandSent.TabIndex = 10;
            this.Lb_CommandSent.Text = "Command Sent";
            // 
            // Lb_AwaitingResponse
            // 
            this.Lb_AwaitingResponse.AutoSize = true;
            this.Lb_AwaitingResponse.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_AwaitingResponse.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_AwaitingResponse.Location = new System.Drawing.Point(12, 480);
            this.Lb_AwaitingResponse.Name = "Lb_AwaitingResponse";
            this.Lb_AwaitingResponse.Size = new System.Drawing.Size(153, 21);
            this.Lb_AwaitingResponse.TabIndex = 11;
            this.Lb_AwaitingResponse.Text = "Awaiting Response";
            // 
            // Lb_ClientError
            // 
            this.Lb_ClientError.AutoSize = true;
            this.Lb_ClientError.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_ClientError.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_ClientError.Location = new System.Drawing.Point(12, 510);
            this.Lb_ClientError.Name = "Lb_ClientError";
            this.Lb_ClientError.Size = new System.Drawing.Size(95, 21);
            this.Lb_ClientError.TabIndex = 12;
            this.Lb_ClientError.Text = "Client Error";
            // 
            // Lb_AbortedConnection
            // 
            this.Lb_AbortedConnection.AutoSize = true;
            this.Lb_AbortedConnection.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_AbortedConnection.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_AbortedConnection.Location = new System.Drawing.Point(12, 542);
            this.Lb_AbortedConnection.Name = "Lb_AbortedConnection";
            this.Lb_AbortedConnection.Size = new System.Drawing.Size(161, 21);
            this.Lb_AbortedConnection.TabIndex = 13;
            this.Lb_AbortedConnection.Text = "Aborted Connection";
            // 
            // Lb_WaitingForCommand
            // 
            this.Lb_WaitingForCommand.AutoSize = true;
            this.Lb_WaitingForCommand.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Lb_WaitingForCommand.Font = new System.Drawing.Font("Montserrat", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_WaitingForCommand.Location = new System.Drawing.Point(12, 573);
            this.Lb_WaitingForCommand.Name = "Lb_WaitingForCommand";
            this.Lb_WaitingForCommand.Size = new System.Drawing.Size(175, 21);
            this.Lb_WaitingForCommand.TabIndex = 14;
            this.Lb_WaitingForCommand.Text = "Waiting for command";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 603);
            this.Controls.Add(this.Lb_WaitingForCommand);
            this.Controls.Add(this.Lb_AbortedConnection);
            this.Controls.Add(this.Lb_ClientError);
            this.Controls.Add(this.Lb_AwaitingResponse);
            this.Controls.Add(this.Lb_CommandSent);
            this.Controls.Add(this.Lb_ConnectionRetry);
            this.Controls.Add(this.Lb_ClientConnected);
            this.Controls.Add(this.Lb_ClientStarted);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Btn_Connect;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Btn_StepperJog;
        private System.Windows.Forms.Button Btn_PathFollow;
        private System.Windows.Forms.Button Btn_ObjectTracking;
        private System.Windows.Forms.Button Btn_GimbalJog;
        private System.Windows.Forms.Button Btn_TestMode;
        private System.Windows.Forms.Label Lb_ClientStarted;
        private System.Windows.Forms.Label Lb_ClientConnected;
        private System.Windows.Forms.Label Lb_ConnectionRetry;
        private System.Windows.Forms.Label Lb_CommandSent;
        private System.Windows.Forms.Label Lb_AwaitingResponse;
        private System.Windows.Forms.Label Lb_ClientError;
        private System.Windows.Forms.Label Lb_AbortedConnection;
        private System.Windows.Forms.Label Lb_WaitingForCommand;
    }
}

