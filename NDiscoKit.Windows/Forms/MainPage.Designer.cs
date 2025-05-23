﻿namespace NDiscoKit.Windows.Forms;

partial class MainPage
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
        blazorWebView = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
        SuspendLayout();
        // 
        // blazorWebView
        // 
        blazorWebView.Dock = DockStyle.Fill;
        blazorWebView.Location = new Point(0, 0);
        blazorWebView.Name = "blazorWebView";
        blazorWebView.Size = new Size(1264, 745);
        blazorWebView.TabIndex = 0;
        blazorWebView.Text = "blazorWebView1";
        // 
        // MainPage
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(1264, 745);
        Controls.Add(blazorWebView);
        MinimumSize = new Size(480, 270);
        Name = "MainPage";
        Text = "NDiscoKit";
        ResumeLayout(false);
    }

    #endregion

    private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView;
}
