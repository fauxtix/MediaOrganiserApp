namespace PromptEditor;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Text = "Form1";

        this.comboBoxFiles = new System.Windows.Forms.ComboBox();
        this.textBoxContent = new System.Windows.Forms.TextBox();
        this.buttonSave = new System.Windows.Forms.Button();

        // comboBoxFiles
        this.comboBoxFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxFiles.FormattingEnabled = true;
        this.comboBoxFiles.Location = new System.Drawing.Point(12, 12);
        this.comboBoxFiles.Name = "comboBoxFiles";
        this.comboBoxFiles.Size = new System.Drawing.Size(776, 28);
        this.comboBoxFiles.TabIndex = 0;

        // textBoxContent
        this.textBoxContent.Location = new System.Drawing.Point(12, 46);
        this.textBoxContent.Multiline = true;
        this.textBoxContent.Name = "textBoxContent";
        this.textBoxContent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.textBoxContent.Size = new System.Drawing.Size(776, 350);
        this.textBoxContent.TabIndex = 1;

        // buttonSave
        this.buttonSave.Location = new System.Drawing.Point(12, 402);
        this.buttonSave.Name = "buttonSave";
        this.buttonSave.Size = new System.Drawing.Size(94, 29);
        this.buttonSave.TabIndex = 2;
        this.buttonSave.Text = "Save";
        this.buttonSave.UseVisualStyleBackColor = true;

        // Form1
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.buttonSave);
        this.Controls.Add(this.textBoxContent);
        this.Controls.Add(this.comboBoxFiles);
        this.Name = "Form1";
        this.Text = "Prompt Editor";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxFiles;
    private System.Windows.Forms.TextBox textBoxContent;
    private System.Windows.Forms.Button buttonSave;
}
