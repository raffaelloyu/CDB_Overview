
Class Application
    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.[Exit]
        ' '   Dim fs1 As FileStream
        '   Dim s1 As StreamWriter
        '   Dim sUser = Environment.UserName

        '   If sUser <> "bzye" Then
        '  If File.Exists("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001\log.txt") Then
        ' fs1 = New FileStream("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001\log.txt", FileMode.Append, FileAccess.Write, FileShare.Write)
        ' Else
        'My.Computer.FileSystem.CreateDirectory("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001")
        'fs1 = New FileStream("\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\Dropbox\ZZZ Cache001\log.txt", FileMode.Create, FileAccess.Write, FileShare.Write)
        ' File.Create("O:\Dropbox\000 Cache001\log.txt")
        'End If

        's1 = New StreamWriter(fs1)
        's1.Write("User " & sUser & " exit CDB Overview with error " & e.ApplicationExitCode & vbCrLf)
        's1.Flush()
        's1.Close()
        'fs1.Close()
        ' End If
        ' Environment.Exit(0)
    End Sub

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

End Class
