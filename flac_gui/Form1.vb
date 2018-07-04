Public Class Form1

    Private Sub InputBrowseBtn_Click(sender As Object, e As EventArgs) Handles InputBrowseBtn.Click
        Dim InputBrowser As New FolderBrowserDialog
        InputBrowser.ShowNewFolderButton = False
        Dim OkAction As MsgBoxResult = InputBrowser.ShowDialog
        If OkAction = MsgBoxResult.Ok Then
            InputTxt.Text = InputBrowser.SelectedPath
        End If
    End Sub

    Private Sub OutputBrowseBtn_Click(sender As Object, e As EventArgs) Handles OutputBrowseBtn.Click
        Dim OutputBrowser As New FolderBrowserDialog
        OutputBrowser.ShowNewFolderButton = True
        Dim OkAction As MsgBoxResult = OutputBrowser.ShowDialog
        If OkAction = MsgBoxResult.Ok Then
            OutputTxt.Text = OutputBrowser.SelectedPath
        End If
    End Sub

    Private Sub StartBtn_Click(sender As Object, e As EventArgs) Handles StartBtn.Click
        If Not My.Computer.FileSystem.DirectoryExists(OutputTxt.Text) Then My.Computer.FileSystem.CreateDirectory(OutputTxt.Text)
        ProgressBar1.Maximum = My.Computer.FileSystem.GetFiles(InputTxt.Text).Count
        ProgressBar1.Value = 0
        For Each File In IO.Directory.GetFiles(InputTxt.Text)
            run_flac(File, OutputTxt.Text + "\" + My.Computer.FileSystem.GetName(File), CompressionLevelChoose.SelectedItem)
            ProgressBar1.PerformStep()
            Me.Update()
        Next
        MsgBox("Finished")
    End Sub
    Private Sub run_flac(Input_File As String, Output_File As String, CompressionLevel As String)
        Dim flacProcessInfo As New ProcessStartInfo
        Dim flacProcess As Process
        flacProcessInfo.FileName = "flac.exe"
        flacProcessInfo.Arguments = "-" & CompressionLevel & " -V """ + Input_File + """ -o """ + Output_File + """"
        flacProcessInfo.CreateNoWindow = True
        flacProcessInfo.RedirectStandardOutput = True
        flacProcessInfo.UseShellExecute = False
        flacProcess = Process.Start(flacProcessInfo)
        flacProcess.WaitForExit()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CompressionLevelChoose.SelectedIndex = My.Settings.CompressionLevel
    End Sub

    Private Sub CompressionLevelChoose_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CompressionLevelChoose.SelectedIndexChanged
        My.Settings.CompressionLevel = CompressionLevelChoose.SelectedIndex
        My.Settings.Save()
    End Sub
End Class
