namespace LoLNotes.Gui.Controls
{
	partial class StatsControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.GameType = new System.Windows.Forms.Label();
			this.MaxElo = new System.Windows.Forms.Label();
			this.Elo = new System.Windows.Forms.Label();
			this.Wins = new System.Windows.Forms.Label();
			this.Losses = new System.Windows.Forms.Label();
			this.Leaves = new System.Windows.Forms.Label();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.flowLayoutPanel1.Controls.Add(this.GameType);
			this.flowLayoutPanel1.Controls.Add(this.MaxElo);
			this.flowLayoutPanel1.Controls.Add(this.Elo);
			this.flowLayoutPanel1.Controls.Add(this.Wins);
			this.flowLayoutPanel1.Controls.Add(this.Losses);
			this.flowLayoutPanel1.Controls.Add(this.Leaves);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(300, 100);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// GameType
			// 
			this.GameType.AutoSize = true;
			this.GameType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GameType.Location = new System.Drawing.Point(3, 0);
			this.GameType.Name = "GameType";
			this.GameType.Size = new System.Drawing.Size(43, 13);
			this.GameType.TabIndex = 5;
			this.GameType.Text = "Type: ";
			// 
			// MaxElo
			// 
			this.MaxElo.AutoSize = true;
			this.MaxElo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaxElo.Location = new System.Drawing.Point(3, 13);
			this.MaxElo.Name = "MaxElo";
			this.MaxElo.Size = new System.Drawing.Size(60, 13);
			this.MaxElo.TabIndex = 0;
			this.MaxElo.Text = "Max Elo: ";
			// 
			// Elo
			// 
			this.Elo.AutoSize = true;
			this.Elo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Elo.Location = new System.Drawing.Point(3, 26);
			this.Elo.Name = "Elo";
			this.Elo.Size = new System.Drawing.Size(33, 13);
			this.Elo.TabIndex = 1;
			this.Elo.Text = "Elo: ";
			// 
			// Wins
			// 
			this.Wins.AutoSize = true;
			this.Wins.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Wins.Location = new System.Drawing.Point(3, 39);
			this.Wins.Name = "Wins";
			this.Wins.Size = new System.Drawing.Size(43, 13);
			this.Wins.TabIndex = 2;
			this.Wins.Text = "Wins: ";
			// 
			// Losses
			// 
			this.Losses.AutoSize = true;
			this.Losses.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Losses.Location = new System.Drawing.Point(3, 52);
			this.Losses.Name = "Losses";
			this.Losses.Size = new System.Drawing.Size(54, 13);
			this.Losses.TabIndex = 3;
			this.Losses.Text = "Losses: ";
			// 
			// Leaves
			// 
			this.Leaves.AutoSize = true;
			this.Leaves.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Leaves.Location = new System.Drawing.Point(3, 65);
			this.Leaves.Name = "Leaves";
			this.Leaves.Size = new System.Drawing.Size(56, 13);
			this.Leaves.TabIndex = 4;
			this.Leaves.Text = "Leaves: ";
			// 
			// StatsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Name = "StatsControl";
			this.Size = new System.Drawing.Size(300, 100);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label MaxElo;
		private System.Windows.Forms.Label Elo;
		private System.Windows.Forms.Label Wins;
		private System.Windows.Forms.Label Losses;
		private System.Windows.Forms.Label Leaves;
		private System.Windows.Forms.Label GameType;
	}
}
