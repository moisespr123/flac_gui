Imports System.Threading

Public Class Form1
    Private bypassCommands As Boolean = False
    Private Commands As String = ""
    Private Sub InputBrowseBtn_Click(sender As Object, e As EventArgs) Handles InputBrowseBtn.Click
        Dim InputBrowser As New FolderBrowserDialog With {
            .ShowNewFolderButton = False
        }
        Dim OkAction As MsgBoxResult = InputBrowser.ShowDialog
        If OkAction = MsgBoxResult.Ok Then
            InputTxt.Text = InputBrowser.SelectedPath
        End If
    End Sub

    Private Sub OutputBrowseBtn_Click(sender As Object, e As EventArgs) Handles OutputBrowseBtn.Click
        Dim OutputBrowser As New FolderBrowserDialog With {
            .ShowNewFolderButton = True
        }
        Dim OkAction As MsgBoxResult = OutputBrowser.ShowDialog
        If OkAction = MsgBoxResult.Ok Then
            OutputTxt.Text = OutputBrowser.SelectedPath
        End If
    End Sub

    Private Sub StartBtn_Click(sender As Object, e As EventArgs) Handles StartBtn.Click
        StartBtn.Enabled = False
        InputTxt.Enabled = False
        OutputTxt.Enabled = False
        InputBrowseBtn.Enabled = False
        OutputBrowseBtn.Enabled = False
        If Not bypassCommands Then
            CompressionLevelChoose.Enabled = False
            EflagCheckbox.Enabled = False
            PflagCheckbox.Enabled = False
        End If
        Dim StartTasks As New Thread(Sub() StartThreads())
        StartTasks.Start()
    End Sub
    Private Sub StartThreads()
        If Not My.Computer.FileSystem.DirectoryExists(OutputTxt.Text) Then My.Computer.FileSystem.CreateDirectory(OutputTxt.Text)
        Dim ItemsToProcess As List(Of String) = New List(Of String)
        For Each File In IO.Directory.GetFiles(InputTxt.Text)
            If IO.Path.GetExtension(File) = ".wav" Or IO.Path.GetExtension(File) = ".flac" Then
                ItemsToProcess.Add(File)
            Else
                If Not My.Computer.FileSystem.FileExists(OutputTxt.Text + "\" + My.Computer.FileSystem.GetName(File)) Then
                    My.Computer.FileSystem.CopyFile(File, OutputTxt.Text + "\" + My.Computer.FileSystem.GetName(File))
                End If
            End If
        Next
        ProgressBar1.BeginInvoke(Sub()
                                     ProgressBar1.Maximum = ItemsToProcess.Count
                                     ProgressBar1.Value = 0
                                 End Sub
        )
        Dim tasks = New Task(ItemsToProcess.Count - 1) {}
        For Counter As Integer = 0 To ItemsToProcess.Count - 1
            Dim args As Array = {ItemsToProcess(Counter), OutputTxt.Text + "\" + My.Computer.FileSystem.GetName(ItemsToProcess(Counter)), My.Settings.CompressionLevel}
            tasks(Counter) = Task.Factory.StartNew(Function() Run_flac(args))
        Next
        Task.WaitAll(tasks)
        StartBtn.BeginInvoke(Sub()
                                 StartBtn.Enabled = True
                                 InputTxt.Enabled = True
                                 OutputTxt.Enabled = True
                                 InputBrowseBtn.Enabled = True
                                 OutputBrowseBtn.Enabled = True
                                 If Not bypassCommands Then
                                     CompressionLevelChoose.Enabled = True
                                     EflagCheckbox.Enabled = True
                                     PflagCheckbox.Enabled = True
                                 End If
                             End Sub)
        MsgBox("Finished")
    End Sub
    Private Function Run_flac(ByVal args As Array) As Boolean
        Dim flacProcessInfo As New ProcessStartInfo
        Dim flacProcess As Process
        flacProcessInfo.FileName = "flac.exe"
        If bypassCommands = True Then
            flacProcessInfo.Arguments = Commands & " -V """ + args(0) + """ -o """ + args(1) + """"
        Else
            flacProcessInfo.Arguments = "-" & args(2) & " " & My.Settings.Eflag & " " & My.Settings.Pflag & " -V """ + args(0) + """ -o """ + args(1) + """"
        End If
        flacProcessInfo.CreateNoWindow = True
        flacProcessInfo.RedirectStandardOutput = True
        flacProcessInfo.UseShellExecute = False
        flacProcess = Process.Start(flacProcessInfo)
        flacProcess.WaitForExit()
        ProgressBar1.BeginInvoke(Sub() ProgressBar1.PerformStep())
        Return True
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If My.Computer.FileSystem.FileExists("flac.exe") Then
            CompressionLevelChoose.SelectedIndex = My.Settings.CompressionLevel
            If Not My.Settings.Eflag = String.Empty Then EflagCheckbox.Checked = True
            If Not My.Settings.Pflag = String.Empty Then PflagCheckbox.Checked = True
            If My.Computer.FileSystem.FileExists("commands.txt") Then
                bypassCommands = True
                Commands = My.Computer.FileSystem.ReadAllText("commands.txt")
                commandsFound.Visible = True
                CompressionLevelChoose.Enabled = False
                EflagCheckbox.Enabled = False
                PflagCheckbox.Enabled = False
            End If
        Else
            MessageBox.Show("flac.exe not found. Exiting...")
            Close()
        End If
    End Sub

    Private Sub CompressionLevelChoose_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CompressionLevelChoose.SelectedIndexChanged
        My.Settings.CompressionLevel = CompressionLevelChoose.SelectedIndex
        My.Settings.Save()
    End Sub

    Private Sub EflagCheckbox_CheckedChanged(sender As Object, e As EventArgs) Handles EflagCheckbox.CheckedChanged
        If EflagCheckbox.Checked Then
            My.Settings.Eflag = "-e"
        Else
            My.Settings.Eflag = String.Empty
        End If
        My.Settings.Save()
    End Sub

    Private Sub PflagCheckbox_CheckedChanged(sender As Object, e As EventArgs) Handles PflagCheckbox.CheckedChanged
        If PflagCheckbox.Checked Then
            My.Settings.Pflag = "-p"
        Else
            My.Settings.Pflag = String.Empty
        End If
        My.Settings.Save()
    End Sub
End Class
