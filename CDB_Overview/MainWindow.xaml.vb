
'Imports System.Windows
'Imports System.Windows.Controls
'Imports System.Windows.Data
Imports System.Windows.Threading
Imports System.Threading
'Imports System.Windows.Markup
Imports System.IO
Imports System.ComponentModel

Imports Visifire.Charts
'Imports System.Net
'Imports System.Globalization
Imports Microsoft.Office.Interop

Imports OSIsoft.AF
Imports OSIsoft.AF.PI
Imports OSIsoft.AF.Asset
Imports OSIsoft.AF.Time
Imports System.Data.OleDb
Imports System.Data

Imports System.Windows.Xps
Class MainWindow
    Private piservers As New PIServers
    Private pisystems As New PISystems
    Private PISystem As PISystem
    Private PIServer As PIServer
    Private srvAF As OSIsoft.AF.PI.PIServer
    Private sServerName As String

    Private sinusoid As OSIsoft.AF.PI.PIPoint
    Private timerange As AFTimeRange
    Private values As AFValues
    Private sFilter As String

    Private sUser As String

    Private blnFirst As Boolean = True
    Private elementList As New BindingList(Of testpu)
    Private chartList As New BindingList(Of Chart)
    Private Delegate Sub SubPrimeDelegate(ByRef kida As testpu)
    Private Delegate Sub SubChartPrimeDelegate(ByRef kida As Chart)

    Private Delegate Sub SubPrimeDelegate_prog()

    Private config_aa As New Xml.XmlDocument
    Private sPageType As String
    Private strXML As String

    Private sLineColors(16) As Brush
    Private foundPoints, foundPoints1, foundPointsld As IEnumerable(Of OSIsoft.AF.PI.PIPoint)
    Private pts, pts1, pts_ld As OSIsoft.AF.PI.PIPointList
    Private WithEvents timer1 As New DispatcherTimer
    Private WithEvents timer2 As New DispatcherTimer
    Private WithEvents timer3 As New DispatcherTimer

    Private sTime_start, sTime_end As Date
    Private times() As AFTime

    Private cha As Chart

    Private rValues_as(6, 0) As Double
    Private sDates_as(6, 0) As String
    Private iPoints_as(6) As String

    Private sDates(0) As String
    Private blnAddMarkers, blnAddLabels, blnAddChart As Boolean
    Private pi_col_xls_tags() As String
    Private sTagDrag As String

    Private iSulfurRemoval As Integer = 1
    Private iScotStack As Integer = 1
    Private iScot As Integer = 1

    Dim fs1 As FileStream
    Dim s1 As StreamWriter
    Private thread As Thread

    Private sAppTime As Date = DateAdd(DateInterval.Hour, 14, Now)

    Private strConnectionString As String = My.Settings.conString
    Private con As New OleDbConnection
    Private cmd As New OleDbCommand
    Private adapter As OleDbDataAdapter '.SqlDataAdapter
    Private mydataset As DataSet

    Private sbasefolder As String = My.Settings.sbasefolder
    Private blnAuthorize As Boolean = False
    Private strSQL_types As String
    Private blnScribe As Boolean = False
    Private iMessage As Integer = 0
    Dim sMessages(9) As String

    Private startPoint As Point
    Private endPoint As Point
    Private rubberband As Shape
    'Private scaletrasform1 As New ScaleTransform

    Public Sub New()


        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponet() call.
        sUser = Environment.UserName

        '  Dim sArgs() As String
        '   sArgs = Environment.GetCommandLineArgs()

        Dim sTempFolder As String
        sTempFolder = "\\pcwpfsv001\share\Dropbox"
        ' sTempFolder = "\\Cdbchqw8fs01.cdbchq.chevrontexaco.net\share\OPS Automation\Apps Folder\CDB_Overview"

        If Not File.Exists(sTempFolder & "\000 Cache001\log.txt") Then
            ' My.Computer.FileSystem.CreateDirectory(sTempFolder & "\000 Cache001")
        End If

        If LCase(sUser) = "jdjw" Or LCase(sUser) = "zoyucao" Then
            btnRefresh.Visibility = Visibility.Visible
        Else
            btnRefresh.Visibility = Visibility.Hidden
        End If

        If sUser <> "zoyucao" Then
            Try
                If File.Exists(sTempFolder & "\ZZZ Cache001\log.txt") Then
                    fs1 = New FileStream(sTempFolder & "\ZZZ Cache001\log.txt", FileMode.Append, FileAccess.Write, FileShare.Write)
                Else
                    My.Computer.FileSystem.CreateDirectory(sTempFolder & "\ZZZ Cache001")
                    fs1 = New FileStream(sTempFolder & "\ZZZ Cache001\log.txt", FileMode.Create, FileAccess.Write, FileShare.Write)
                    ' File.Create("O:\Dropbox\000 Cache001\log.txt")
                End If

                s1 = New StreamWriter(fs1)
                s1.Write("New Version User " & sUser & " Logged on " & Now & vbCrLf)
                s1.Flush()
                s1.Close()
                fs1.Close()
            Catch ex As Exception

            End Try
        End If

        ' check if acces to the folder
        Dim secTemp As Security.AccessControl.DirectorySecurity
        'Dim rulcol As Collection
        Dim stemp As String
        Dim aorsxml As New Xml.XmlDocument
        Dim nnode As Xml.XmlNode

        Try
            secTemp = Directory.GetAccessControl(sbasefolder) '.GetAccessRules(True, True, GetType(Security.Principal.NTAccount))
            Try
                ' check connection to DB
                con.ConnectionString = strConnectionString

                cmd.CommandType = System.Data.CommandType.Text
                cmd.Connection = con
                con.Open()
                cmd.CommandText = "SELECT * FROM users WHERE cai='" & sUser & "'"
                adapter = New OleDbDataAdapter
                mydataset = New DataSet
                adapter.SelectCommand = cmd
                adapter.Fill(mydataset)
                stemp = mydataset.GetXml
                stemp = Replace(stemp, "&lt;", "<")
                stemp = Replace(stemp, "&gt;", ">")
                aorsxml.LoadXml(stemp)

                'check if Plant is authorized

                nnode = aorsxml.DocumentElement.SelectSingleNode("Table/types/areas[area='Plant 工厂']/area")
                If IsNothing(nnode) Then
                    ' no access to Plant events
                    blnScribe = False
                Else
                    blnScribe = True
                End If

            Catch ex As Exception
                blnScribe = False
            End Try

        Catch ex As Exception
            ' MsgBox("Not access to the SCRIBE folder")
            blnScribe = False
        End Try

        Try
            If con.State = ConnectionState.Open Then
                con.Close()
            End If
        Catch ex As Exception

        End Try

        btnDataExport.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf navhomeWEB))
        'imgPrintSulfur.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf printSulfur))
        imgPrint.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf printReport))
        imgSave.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf saveXPS))

        If blnScribe Then
            btnPumps.Visibility = Visibility.Visible
            btnScribe.Visibility = Visibility.Visible
            btnScribe.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf navhomeWEB))
            btnPumps.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf navhomeWEB))
        Else
            btnPumps.Visibility = Visibility.Hidden
            btnScribe.Visibility = Visibility.Hidden
        End If

        PIServer = piservers("pnwpappv003")
        srvAF = OSIsoft.AF.PI.PIServer.FindPIServer("pnwpappv003")
        sServerName = "pnwpappv003"

        sLineColors(0) = New SolidColorBrush(ColorConverter.ConvertFromString("#FF6B8EAD")) ' "#FF6B8EAD" ' Brushes.DarkBlue 'Violat
        sLineColors(1) = New SolidColorBrush(ColorConverter.ConvertFromString("#FF6B8EAD")) ' Brushes.Black
        sLineColors(2) = Brushes.DarkSlateGray
        sLineColors(3) = Brushes.DarkBlue
        sLineColors(4) = Brushes.DarkMagenta
        sLineColors(5) = Brushes.Indigo
        sLineColors(6) = Brushes.DeepPink
        sLineColors(7) = Brushes.Beige
        sLineColors(8) = Brushes.Brown
        sLineColors(9) = Brushes.Gray
        sLineColors(10) = Brushes.Olive
        sLineColors(11) = Brushes.Blue
        sLineColors(12) = Brushes.Purple
        sLineColors(13) = Brushes.Salmon
        sLineColors(14) = Brushes.Blue
        sLineColors(15) = Brushes.Aquamarine

        sMessages(0) = "总是遵守川东北天然气项目安全黄金法则"
        sMessages(1) = "进入厂区和井场时总是正确使用硫化氢防护设备"
        sMessages(2) = "总是执行停工指令"
        sMessages(3) = "在超过1.8米的高空作业时，总是遵守高空作业的要求。"
        sMessages(4) = "总是遵守受限空间准入的程序开展受限空间工作"
        sMessages(5) = "总是遵守能量隔离和挂牌上锁程序"
        sMessages(6) = "在生产区域开展明火热工作业时总是遵守热工作业程序"
        sMessages(7) = "绝不参与打架，肢体冲突或威胁他人。"
        sMessages(8) = "绝不允许吸食毒品或酒后作业"

        sPageType = "Operations"
        '
        Try
            Dim sTags(0) As String

            sTags(0) = "NGP_GROSS_SALES_GAS_RATE_NM3H_CALC.PV"
            foundPoints = OSIsoft.AF.PI.PIPoint.FindPIPoints(srvAF, sTags)
            pts = New OSIsoft.AF.PI.PIPointList(foundPoints)

            ReDim pi_col_xls_tags(1)

            pi_col_xls_tags(1) = "NGP_GROSS_SALES_GAS_RATE_NM3H_CALC.PV"
        Catch ex As Exception

        End Try


        ''  If Now.Date > CDate("2/20/2019") Then
        ''   imgHoliday.Visibility = Visibility.Hidden
        ''   Else
        ''   imgHoliday.Visibility = Visibility.Visible
        ''   End If
        ' comUnits.Items.Clear()
        '  comUnits.Items.Add("MMSCMD")
        ' comUnits.Items.Add("MMSFMD")

        Dim inum As Integer
        inum = 5000

        ReDim rValues_as(8, inum)
        ReDim sDates_as(8, inum)
        ReDim iPoints_as(8)

        Call createPage("", "", sPageType)

        timer1.IsEnabled = True
        timer1.Interval = TimeSpan.FromMinutes(30)
        timer1.Start()

        timer2.IsEnabled = True
        timer2.Interval = TimeSpan.FromSeconds(7)
        timer2.Start()

        'timer3.IsEnabled = True
        'timer3.Interval = TimeSpan.FromSeconds(1)
        'timer3.Start()

    End Sub

    Private Sub mycanvas_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles mycanvas.MouseLeftButtonDown
        If My.Computer.Keyboard.CtrlKeyDown Then
            If Not mycanvas.IsMouseCaptured Then
                startPoint = e.GetPosition(mycanvas)
                Mouse.Capture(mycanvas)
            End If
        End If
    End Sub

    Private Sub mycanvas_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles mycanvas.MouseLeftButtonUp
        Dim xxscale, yyscale As Double
        If My.Computer.Keyboard.CtrlKeyDown Then
            If mycanvas.IsMouseCaptured Then
                If Not IsNothing(rubberband) Then
                    Dim move As TranslateTransform = New TranslateTransform(-startPoint.X, -startPoint.Y)
                    xxscale = mycanvas.Width / (rubberband.Width)
                    yyscale = mycanvas.Height / (rubberband.Height)
                    Dim group As New TransformGroup
                    Dim myscale As ScaleTransform = New ScaleTransform(xxscale, yyscale, 0, 0)
                    group.Children.Add(move)
                    group.Children.Add(myscale)

                    mycanvas.RenderTransform = group
                    mycanvas.Children.Remove(rubberband)
                    rubberband = Nothing
                    mycanvas.ReleaseMouseCapture()
                End If
            End If
        End If
    End Sub
    Private Sub mycanvas_MouseMove(sender As Object, e As MouseEventArgs) Handles mycanvas.MouseMove
        If My.Computer.Keyboard.CtrlKeyDown Then
            If mycanvas.IsMouseCaptured Then
                endPoint = e.GetPosition(mycanvas)
                If IsNothing(rubberband) Then
                    rubberband = New Rectangle()
                    rubberband.Stroke = Brushes.Red
                    mycanvas.Children.Add(rubberband)
                End If
                rubberband.Width = Math.Abs(startPoint.X - endPoint.X)
                rubberband.Height = Math.Abs(startPoint.Y - endPoint.Y)

                Dim Left As Double = Math.Min(startPoint.X, endPoint.X)
                Dim Top As Double = Math.Min(startPoint.Y, endPoint.Y)
                Canvas.SetLeft(rubberband, Left)
                Canvas.SetTop(rubberband, Top)
            End If
        End If
    End Sub


    Private Sub saveXPS()
        Dim sFile As String = "C:\Temp\snapshot" & Now.Millisecond & ".xps"
        Dim package As Packaging.Package
        package = Packaging.Package.Open(sFile, FileMode.Create, FileAccess.ReadWrite)
        Dim doc As New Packaging.XpsDocument(package)
        Dim writer As XpsDocumentWriter
        writer = Packaging.XpsDocument.CreateXpsDocumentWriter(doc)
        writer.Write(mycanvas)
        doc.Close()
        package.Close()
        System.Diagnostics.Process.Start(sFile)

    End Sub

    Private Sub printReport()
        Dim prdlg As New PrintDialog
        ' Dim capabilities As System.Printing.
        Dim capabilities As Printing.PrintCapabilities
        Dim scale As Double
        Dim mys As Size
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Double

        '   Dim root As New Canvas

        xxscale = mycanvas.Width
        yyscale = mycanvas.Height
        Dim transform As Transform = mycanvas.LayoutTransform

        '   Dim package As Packaging.Package
        '  package = Packaging.Package.Open("C:\Projects\SCRIBE\testxaml.xps", FileMode.Create, FileAccess.ReadWrite)
        '  Dim doc As New Packaging.XpsDocument(package)
        '   Dim writer As XpsDocumentWriter
        '  writer = Packaging.XpsDocument.CreateXpsDocumentWriter(doc)
        '  writer.Write(mycanvas)
        ' doc.Close()
        '  package.Close()

        '      Me.WindowState = WindowState.Maximized

        If prdlg.ShowDialog Then
            '     If False Then

            capabilities = prdlg.PrintQueue.GetPrintCapabilities(prdlg.PrintTicket)
            scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / mycanvas.ActualWidth, capabilities.PageImageableArea.ExtentHeight / mycanvas.ActualHeight)

            mycanvas.LayoutTransform = New ScaleTransform(scale, scale)
            ' myscale = New ScaleTransform(scale, scale, 0, 0)
            ' mycanvas.RenderTransform = myscale

            mys = New Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight)

            mycanvas.Measure(mys)
            '  mycanvas.Arrange(New Rect(New Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), mys))
            mycanvas.Arrange(New Rect(New Point(20, 100), mys))

            prdlg.PrintVisual(mycanvas, "")

            mycanvas.LayoutTransform = transform
        End If

    End Sub
    Private Sub printSulfur()

    End Sub
    Private Sub navhomeWEB(sender As Object, e As RoutedEventArgs)
        ' Dim sTag As String = sender.tag
        Dim exePath As String
        ' Dim id As Integer

        Try
            If sender.tag = "CDBDataExport" Then
                exePath = "\\pcwpfsv001\share\PI Process Book Folders\SCRIBE\PIDataExport\PIDataExport.application"
            End If

            If sender.tag = "SCRIBE" Then
                exePath = "\\pcwpfsv001\share\PI Process Book Folders\SCRIBE\SCRIBE_a\SCRIBE_a.application"
            End If

            If sender.tag = "NGPPumps" Then
                exePath = "\\pcwpfsv001\share\PI Process Book Folders\SCRIBE\NGPPumps_a\NGPPumps_a.Application"
            End If

            Process.Start(exePath)
            ' id = Process.Start(exePath).Handle
        Catch ex As Exception

        End Try

    End Sub
    Private Sub writeToLog(ByVal sDisplay As String)
        Dim sTempFolder As String
        If sUser <> "zoyucao" Then
            Try
                If File.Exists(sTempFolder & "\ZZZ Cache001\log.txt") Then
                    fs1 = New FileStream(sTempFolder & "\ZZZ Cache001\log.txt", FileMode.Append, FileAccess.Write, FileShare.Write)
                Else
                    My.Computer.FileSystem.CreateDirectory(sTempFolder & "\ZZZ Cache001")
                    fs1 = New FileStream(sTempFolder & "\ZZZ Cache001\log.txt", FileMode.Create, FileAccess.Write, FileShare.Write)
                    ' File.Create("O:\Dropbox\000 Cache001\log.txt")
                End If

                s1 = New StreamWriter(fs1)
                s1.Write("User " & sUser & " called display " & sDisplay & " " & Now & vbCrLf)
                s1.Flush()
                s1.Close()
                fs1.Close()
            Catch ex As Exception

            End Try
        End If
    End Sub
    Public Sub startPro()
        Dim newdia As New Progress
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub
    Private Sub callPro()
        thread = New Thread(AddressOf startPro)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
    End Sub
    Public Sub createPage(ByVal sArea As String, ByVal sPipe As String, ByVal sPageType As String)

        Dim root As New DependencyObject
        Dim kids As IEnumerable
        Dim sName As String
        Dim sTagName As String
        Dim newbutt As New Button
        Dim newtxt As New TextBox
        Dim newrec As New Rectangle
        Dim newimage As New Image
        ' Dim config_aa As Xml.XmlDocument
        Dim newelem As testpu
        Dim stemp As String
        Dim sType As String

        Dim sEX As String
        Dim sScale As String
        Dim sEqu As String
        Dim sTag As String
        Dim sHiHi, sLoLo As String

        Dispatcher.Invoke(DispatcherPriority.Render, New SubPrimeDelegate_prog(AddressOf callPro))

        '   filename = AppDomain.CurrentDomain.BaseDirectory & "XAML\" & sPageType & ".xaml"
        config_aa.Load(AppDomain.CurrentDomain.BaseDirectory & "XML\" & sPageType & "_map.txt")

        elementList.Clear()
        chartList.Clear()

        root = mycanvas

        mycanvas.Width = 1880
        mycanvas.Height = 1090

        kids = LogicalTreeHelper.GetChildren(mycanvas)

        For Each kid In kids

            sName = kid.Name.ToString

            If kid.GetType().Name = "Button" Then
                newbutt = LogicalTreeHelper.FindLogicalNode(root, sName)
                'If sName = "bt_Dismiss" Then ' TCPL
                If sName = "btdismiss" Then
                    ' newbutt.AddHandler(PreviewMouseDownEv New RoutedEventHandler(AddressOf closeWin))
                ElseIf sName = "bt_Quad0" Then
                    '   newbutt.AddHandler(PreviewMouseDownEv New RoutedEventHandler(AddressOf Quad0))
                ElseIf sName = "bt_Quad1" Then
                    '   newbutt.AddHandler(PreviewMouseDownEv New RoutedEventHandler(AddressOf Quad1))
                ElseIf sName = "bt_Quad2" Then
                    '   newbutt.AddHandler(PreviewMouseDownEv New RoutedEventHandler(AddressOf Quad2))
                ElseIf sName = "bt_Quad3" Then
                    '  newbutt.AddHandler(PreviewMouseDownEv New RoutedEventHandler(AddressOf Quad3))
                ElseIf sName = "btnStatus" Then
                ElseIf sName = "btnAnalog" Then
                    '  newbutt.AddHandler(PreviewMouseDownEv New RoutedEventHandler(AddressOf callAnalog))
                ElseIf InStr(newbutt.Content.ToString, "Alarms") > 0 Then
                    'newbutt.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf openAlarms))
                End If
            ElseIf kid.GetType().Name = "Rectangle" Then
                Try
                    sTagName = kid.Tag.ToString
                    sName = kid.Name.ToString
                    newrec = LogicalTreeHelper.FindLogicalNode(root, sName)

                    If Not (config_aa.SelectSingleNode("variables/input[@name='" & sName & "']") Is Nothing) Then
                        newelem = testpu.CreateNewElement
                        AddHandler newelem.PropertyChanged, AddressOf newelem_PropertyChanged

                        sType = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("type").Value
                        sEX = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("ext").Value

                        newelem.ElementName = sName
                        newelem.ElementTag = sTagName

                        sTag = sTagName & "." & sEX

                        Try
                            ' If srv.PIPoints(sTag).Data.CurrentValue().IsGood Then
                            newelem.ElementPITag = sTag
                            stemp = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("type").Value
                            newelem.ElementType = Replace(stemp, "CMX\", "")
                            stemp = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("ext").Value

                            newelem.ElementXML = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']")
                            elementList.Add(newelem)
                            newelem.ElementObject = newrec
                            newrec.DataContext = newelem.ElementXML
                        Catch ex As Exception

                        End Try

                    End If

                Catch ex As Exception

                End Try

            ElseIf kid.GetType().Name = "TextBox" Then
                sTagName = kid.Tag.ToString
                newtxt = LogicalTreeHelper.FindLogicalNode(root, sName)

                If Not (config_aa.SelectSingleNode("variables/input[@name='" & sName & "']") Is Nothing) Then
                    newelem = testpu.CreateNewElement
                    AddHandler newelem.PropertyChanged, AddressOf newelem_PropertyChanged

                    sType = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("type").Value
                    sEX = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("ext").Value

                    Try
                        sScale = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("scale").Value
                    Catch ex As Exception
                        sScale = "1.0"
                    End Try
                    newelem.ElementScale = sScale

                    ' HiHi
                    Try
                        sHiHi = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("hihi").Value
                        sHiHi = sHiHi
                    Catch ex As Exception
                        sHiHi = "none"
                    End Try
                    newelem.ElementHiHI = sHiHi

                    ' LoLo
                    Try
                        sLoLo = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("lolo").Value
                    Catch ex As Exception
                        sLoLo = "none"
                    End Try
                    newelem.ElementLoLo = sLoLo

                    If sType = "analog_equ" Then
                        Try
                            sEqu = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("equ").Value
                        Catch ex As Exception
                            sEqu = ""
                        End Try

                        sTagName = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("var").Value
                        newtxt.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf getTrend))
                    Else
                        sEqu = ""
                        'modify by Yutecxa 2022-8-5 特殊处理硫磺总库存字段
                        If sName = "txtTank1_inv" Then
                            sTag = sTagName
                        Else
                            sTag = sTagName & "." & sEX
                        End If
                        newelem.ElementPITag = sTag
                        newtxt.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf getTrend))
                    End If

                    newelem.ElementEqu = sEqu

                    newelem.ElementName = sName
                    newelem.ElementTag = sTagName


                    Try
                        ' If srv.PIPoints(sTag).Data.CurrentValue().IsGood Then

                        '  stemp = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("type").Value
                        newelem.ElementType = Replace(sType, "CMX\", "")
                        '   stemp = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("ext").Value

                        newelem.ElementXML = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']")
                        elementList.Add(newelem)
                        newelem.ElementObject = newtxt
                        newtxt.DataContext = newelem.ElementXML
                    Catch ex As Exception

                    End Try

                End If

            ElseIf kid.GetType().Name = "Image" Then
                sTagName = ""
                sName = ""

                Try
                    sTagName = kid.Tag.ToString
                    sName = kid.Name.ToString


                    If InStr(sName, "_status") > 0 Then 'klm_status 
                        newimage = LogicalTreeHelper.FindLogicalNode(root, sName)
                        ' newimage.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf openDisplay))
                    End If

                    If Not (config_aa.SelectSingleNode("variables/input[@name='" & sName & "']") Is Nothing) Then

                        newimage = LogicalTreeHelper.FindLogicalNode(root, sName)
                        If InStr(sName, "gdsvalve") > 0 Then
                            '  newimage.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf getStatus))
                            newimage.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf getTrendStatus))
                        End If


                        newelem = testpu.CreateNewElement
                        AddHandler newelem.PropertyChanged, AddressOf newelem_PropertyChanged
                        elementList.Add(newelem)
                        newelem.ElementName = sName
                        newelem.ElementTag = sTagName

                        stemp = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("type").Value

                        newelem.ElementType = Replace(stemp, "CMX\", "")
                        newelem.ElementXML = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']")

                        sTag = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("var").Value '& ".cursta"

                        newelem.ElementPITag = sTag
                        newelem.ElementObject = newimage
                        newimage.DataContext = newelem.ElementXML

                    End If
                Catch ex As Exception

                End Try



            ElseIf kid.GetType().Name = "Chart" Then
                If sPipe <> "PLMALL" Then
                    sName = kid.Name.ToString

                    If Not (config_aa.SelectSingleNode("variables/chart[@name='" & sName & "']") Is Nothing) Then
                        ' kid.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf openPLM))
                        chartList.Add(kid)
                    End If
                End If
            End If

        Next

        For Each kid In elementList
            Try
                Dispatcher.Invoke(DispatcherPriority.Background, TimeSpan.FromSeconds(3), New SubPrimeDelegate(AddressOf ThreadStartTimer_kid), kid)
            Catch ex As Exception
                kid = Nothing
            End Try
        Next



        If sPageType = "Operations" Then
            Call createOperations()
        End If

        thread.Abort()
        thread = Nothing
    End Sub
    Public Sub createOperations()
        Call refreshChart("chart_flows_in")
    End Sub
    Private Sub refreshChart(ByVal sChartName As String)
        Dim iops, iPoints As Integer
        Dim sTags(2) As String

        '    Dim sChartName As String
        Dim ileft, iTop, iWidth, iHeight As Integer
        Dim nSeries, nSeries_from As Integer
        Dim chatype As RenderAs
        Dim sColors() As Brush
        Dim sSeriesNames() As String
        '    Dim iPoints As Integer

        Dim sTYpes(8) As RenderAs
        '  Dim cha As Chart

        strXML = "<variables>"
        strXML = strXML & "<input trend='yes' var='NGP_GROSS_SALES_GAS_RATE_NM3H_CALC.PV' ext='PV'><value/><time/></input>"
        sTags(0) = "NGP_GROSS_SALES_GAS_RATE_NM3H_CALC.PV"
        '  strXML = strXML & "<input trend='yes' var='XHN_GAS-PLNT_MTR-SKID-MTR-2_IFRT.PV' ext='PV'><value/><time/></input>"
        '   sTags(1) = "XHN_GAS-PLNT_MTR-SKID-MTR-2_IFRT.PV"
        '  strXML = strXML & "<input trend='yes' var='XHN_GAS-PLNT_MTR-SKID-MTR-3_IFRT.PV' ext='PV'><value/><time/></input>"
        '   sTags(2) = "XHN_GAS-PLNT_MTR-SKID-MTR-3_IFRT.PV"
        strXML = strXML & "</variables>"

        '    foundPoints = OSIsoft.AF.PI.PIPoint.FindPIPoints(srvAF, sTags)
        '     pts = New OSIsoft.AF.PI.PIPointList(foundPoints)

        sTime_end = Now
        sTime_start = DateAdd(DateInterval.Hour, -24, sTime_end)

        ' If False Then
        Call getChartData(0, strXML, iops, iPoints, pts)

        ''    sChartName = "chart_flows_in"

        'Height="228" Canvas.Left="1330" Stroke="Gray" Canvas.Top="794" Width="489"
        'Height="228" Canvas.Left="1309" Stroke="Gray" Canvas.Top="794" Width="529"
        ileft = 1320
        iTop = 780
        iWidth = 510
        iHeight = 242
        chatype = RenderAs.StackedColumn
        nSeries = 1
        nSeries_from = 0
        ReDim sColors(nSeries)
        ReDim sSeriesNames(nSeries)
        ReDim sTYpes(nSeries)
        iPoints = UBound(sDates) '- 2

        For ii1 = 1 To 1
            sTYpes(ii1) = RenderAs.Line
            sColors(ii1) = sLineColors(ii1)
            sSeriesNames(ii1) = "Total CMS Flow"
        Next

        blnAddMarkers = False
        blnAddLabels = False
        blnAddChart = True

        Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, "SM3/HR")
        cha.AxesY(0).AxisMaximum = 400000
        cha.AxesY(0).Interval = 50000

        cha.AxesY(1).AxisMaximum = 400000
        cha.AxesY(1).Interval = 50000
    End Sub
    Private Sub getChartData(ByVal iGatherType As Integer, ByVal strXML As String, ByRef iops As Integer, ByRef inum As Integer, ByVal pts As OSIsoft.AF.PI.PIPointList)
        Dim sEx(4) As String
        Dim avalues As AFValues
        Dim avalue As AFValue
        Dim ii As Integer
        Dim config_x As New Xml.XmlDocument
        Dim timerange1 As AFTimeRange

        '  Dim results As OSIsoft.AF.AFListResults(Of OSIsoft.AF.PI.PIPoint, OSIsoft.AF.Asset.AFValue)

        '  results = pts.CurrentValue


        Try
            '   config_x.LoadXml(strXML)

            '   Dim ii1 As Integer = 1
            '   For Each nnode In config_x.SelectNodes("//input")
            ' sTitle = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & nnode.attributes.getnameditem("ext").value.ToString
            '  sTitle = UCase(nnode.attributes.getnameditem("var").value.ToString)
            'ReDim Preserve pi_col_xls_tags(ii1)
            'pi_col_xls_tags(ii1) = sTitle
            ' ii1 = ii1 + 1
            '  Next

            iops = 1 'ii1 - 1
            'iops = 4
            If iGatherType = 0 Then
                '    inum = 5000
                '   itemp = 10
                '  ReDim rValues_as(8, inum)
                '   ReDim sDates_as(8, inum)
                '   ReDim iPoints_as(8)

                timerange1 = New AFTimeRange(sTime_start, sTime_end, Globalization.CultureInfo.CurrentCulture)

                inum = 0
                For i = 0 To iops - 1
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, pi_col_xls_tags(i + 1), sinusoid) Then
                        'avalues = pts(i).InterpolatedValuesByCount(timerange1, 500, "", False)
                        avalues = sinusoid.InterpolatedValuesByCount(timerange1, 500, "", False)
                        ii = 0
                        For Each avalue In avalues
                            rValues_as(i, ii) = avalue.Value
                            sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                            ii = ii + 1
                        Next
                        iPoints_as(i) = ii - 1
                    End If
                    ' End If
                Next
            Else
                '    inum = times.Count
                '    itemp = 10
                '    ReDim rValues_as(8, inum)
                '    ReDim sDates_as(8, inum)
                '   ReDim iPoints_as(8)

                inum = 0
                For i = 0 To iops - 1
                    '  If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, pi_col_xls_tags(i + 1), sinusoid) Then
                    ' avalues = sinusoid.InterpolatedValuesAtTimes(times, "", False)
                    '  avalues = sinusoid.PlotValues(timerange1, 500)
                    'avalues = pts(i).InterpolatedValuesByCount(timerange1, 500, "", False)
                    ii = 0
                    For Each avalue In avalues
                        rValues_as(i, ii) = avalue.Value
                        sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                        ii = ii + 1
                    Next
                    iPoints_as(i) = ii - 1
                    ' End If
                Next
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub btnDismiss_Click(sender As Object, e As RoutedEventArgs) Handles btnDismiss.Click
        'Me.Close()
        System.Windows.Application.Current.Shutdown(11)
        'Environment.Exit(10)
    End Sub


    Private Sub createNewChart(ByRef cha As Chart, ByVal chaName As String, ByVal iLeft As Integer, ByVal iTop As Integer,
                               ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal nSeries As Integer, ByVal nSeries_from As Integer,
                               ByVal sSeriesNames() As String,
                               ByVal chatype() As RenderAs, ByVal sLineColor() As Brush, ByVal iPoints As Integer, ByVal sYTitle As String)

        Dim dp As DataPoint
        Dim stemp As String
        Dim itemp As Integer = 0


        '  Dim cha As New Chart

        ' check if chart exists

        ' check if chart exists
        If IsNothing(LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)) Then
            cha = New Chart
        Else
            cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)
            mycanvas.Children.Remove(cha)
            cha = Nothing
            cha = New Chart
        End If

        '  If blnAddChart Then
        cha.AnimationEnabled = True
        mycanvas.Children.Add(cha)
        '  End If

        Canvas.SetLeft(cha, iLeft)
        Canvas.SetTop(cha, iTop)

        cha.Width = iWidth
        cha.Height = iHeight

        cha.BorderThickness = New Thickness(0)
        cha.IndicatorEnabled = True
        cha.LightingEnabled = False
        cha.ShadowEnabled = False

        cha.Background = Brushes.Transparent

        Dim plt As New PlotArea
        plt.ShadowEnabled = False
        '  plt.Background = Brushes.Black

        cha.PlotArea = plt
        cha.Name = chaName
        cha.Theme = "Theme1"
        cha.ScrollingEnabled = False
        ' cha.ZoomingEnabled = True
        '  cha.ColorSet = "Visifire2"

        Dim lg As New Legend
        cha.Legends.Add(lg)
        lg.Background = Brushes.Transparent
        lg.LightingEnabled = False
        lg.BorderThickness = New Thickness(0)
        lg.ShadowEnabled = False



        ' create axis 
        Dim myXax As New Axis
        myXax.AxisType = AxisTypes.Primary
        '   myXax.ValueFormatString = "HH:mm"

        Dim myaxL As AxisLabels = New AxisLabels
        myaxL.FontColor = Brushes.Black
        myXax.AxisLabels = myaxL

        Dim mygr As New ChartGrid
        mygr.LineStyle = LineStyles.Dashed
        mygr.LineThickness = 0.5
        mygr.LineColor = Brushes.DarkGray
        myXax.Grids.Add(mygr)

        ''   myXax.AxisMinimum = FormatNumber(rDates.Average() * 0.8, 0)
        cha.AxesX.Add(myXax)

        Dim myYax As New Axis
        myYax.AxisType = AxisTypes.Primary

        Dim myaxLY As AxisLabels = New AxisLabels
        myaxLY.FontColor = Brushes.Black
        myYax.AxisLabels = myaxLY

        Dim mygr1 As New ChartGrid
        mygr1.LineStyle = LineStyles.Dashed
        mygr1.LineThickness = "0.50"
        mygr1.LineColor = Brushes.DarkGray
        myYax.Grids.Add(mygr1)
        myYax.Title = sYTitle
        cha.AxesY.Add(myYax)

        'title
        Dim myTitle As New Title
        cha.Titles.Add(myTitle)

        ' secondary Y
        Dim myYax1 As New Axis
        myYax1.AxisType = AxisTypes.Secondary
        cha.AxesY.Add(myYax1)

        '  cha.AxesX.ValueFormatString = "MM/dd"
        '   Else
        '   cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)
        '   cha.Series.Clear()
        '   End If

        For i = nSeries_from To nSeries_from + nSeries - 1
            Dim ser As New DataSeries
            ser.RenderAs = chatype(i + 1)
            ser.LabelEnabled = False
            ser.MarkerEnabled = False
            ser.LightingEnabled = True


            If blnAddMarkers Then
                ser.MarkerEnabled = True
            End If

            If blnAddLabels Then
                ser.LabelEnabled = True
            End If

            If ser.RenderAs = RenderAs.Line Or ser.RenderAs = RenderAs.Spline Then
                '     ser.LabelEnabled = True
                If blnAddMarkers Then
                    ser.MarkerEnabled = True
                Else
                    ser.MarkerEnabled = False
                End If
                ser.XValueType = ChartValueTypes.DateTime
                ser.ToolTipText = "#YValue, #Series"
            End If

            ser.MarkerColor = Brushes.White
            ser.MarkerBorderColor = Brushes.Black
            ser.MarkerSize = 10
            If Not IsNothing(sLineColor(i + 1)) Then
                If sLineColor(i + 1).ToString <> Brushes.Transparent.ToString Then
                    ser.Color = sLineColor(i + 1)
                End If
            End If
            ser.LineThickness = 3
            ser.ShadowEnabled = True
            ser.LightingEnabled = False
            ser.Name = sSeriesNames(i + 1)
            ser.DataPoints.Clear()
            ''   AddHandler ser.MouseLeftButtonDown, AddressOf cha_details
            cha.Series.Add(ser)

            Dim itimes As Integer = 0
            stemp = ""
            Dim rtemp As Double = 100000000.0
            If iPoints = 0 Then iPoints = itimes - 1

            'For j = 0 To iPoints - 1
            For j = 0 To iPoints_as(i) - 1
                dp = New DataPoint
                Try
                    dp.YValue = FormatNumber(rValues_as(i, j), 2)
                    If ser.RenderAs = RenderAs.Column Then
                        '  dp.AxisXLabel = sDates(j)
                        dp.AxisXLabel = sDates_as(i, j)
                        ser.LabelEnabled = True
                        ser.MarkerEnabled = True
                    ElseIf ser.RenderAs = RenderAs.Pie Then
                        dp.AxisXLabel = sSeriesNames(j)
                        dp.Color = sLineColor(j)
                        ser.ShowInLegend = True
                        lg.DockInsidePlotArea = True
                        ser.LabelStyle = LabelStyles.Inside
                        ser.LabelText = "#YValue"
                        '   ser.LabelEnabled = True
                        '   ser.MarkerEnabled = True
                    Else
                        ' dp.XValue = sDates(j)
                        dp.XValue = sDates_as(i, j)
                        cha.AxesX(0).ValueFormatString = "HH:mm"
                    End If

                    ser.DataPoints.Add(dp)
                    If rtemp > rValues_as(i + 1, j) Then
                        rtemp = rValues_as(i + 1, j)
                    End If
                Catch ex As Exception

                End Try
            Next
        Next

    End Sub

    Private Sub bntGranulator_Click(sender As Object, e As RoutedEventArgs) Handles bntGranulator.Click
        Dim neww As New Granulator
        neww.Show()

    End Sub
    Private Sub getWhole(ByRef sValue As String, ByRef iValue As Integer, ByRef sValie_i As String, ByRef rValue As Single)
        Dim itemp As Integer

        itemp = InStr(sValue, ".")
        iValue = CInt(Mid(sValue, 1, itemp))
        If iValue < 10 Then
            sValie_i = "0" & iValue
        Else
            sValie_i = iValue
        End If
        rValue = CSng(sValue) - iValue

    End Sub
    Private Sub ThreadStartTimer_kid(ByRef kida As testpu)
        ' Dim kid As testpu
        Dim sName As String = ""
        Dim root As New DependencyObject

        Dim newobj As New Object
        Dim objlist As New Collection

        Dim ncount As Integer
        Dim update_icount_dt As Integer = 0
        Dim dTime As TimeSpan

        Dim myP As AFValue
        Dim element As Xml.XmlNode
        Dim sEx As String
        Dim sType As String
        Dim atemp() As String
        'Dim atemp1() As String
        Dim sTag1, sTag2 As String
        Dim rand As Random
        Dim rScale As Single
        Dim mytxt As TextBox

        dTime = (Now - kida.ElementTime)

        'modified by yutecxa : log for test
        If kida.ElementName = "txtTank1_inv" Then
            Console.WriteLine(kida.ElementName)
        End If

        If dTime > TimeSpan.FromSeconds(20) Then
            rand = New Random(1)
            '    itemp = rand.Next(0, 20)
            kida.ElementTime = DateAdd(DateInterval.Second, rand.Next(0, 20), Now)
            Try
                sType = kida.ElementType
                'GSV_intg_day_current
                sEx = kida.ElementXML.Attributes.GetNamedItem("ext").Value

                If sType = "analog" Or sType = "batmtr" Or sType = "tank" Then

                    If kida.ElementName = "txtChartTimer" Then
                        'all refreshChart("chart_flows_in")
                        kida.ElementXML.SelectSingleNode("PV").InnerText = Now
                        Call UpdateLiveChart("chart_flows_in")
                    Else
                        If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                            myP = sinusoid.CurrentValue()
                            rScale = CSng(kida.ElementScale)
                            '  kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                            If myP.IsGood Then
                                element = kida.ElementXML.Clone
                                Call elementTest(kida, element, myP.Value * rScale, sType, sEx)
                                kida.ElementXML = element
                            Else
                                kida.ElementXML.SelectSingleNode(sEx).InnerText = "ERR"
                                kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "Red"
                            End If

                        Else
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "Yellow"
                            kida.ElementXML.SelectSingleNode("PV").InnerText = "N/A"
                        End If
                    End If
                ElseIf sType = "analog_equ" Then

                    Call processEqu(kida)
                    If kida.ElementHiHI <> "none" And IsNumeric(kida.ElementHiHI) And IsNumeric(kida.ElementXML.SelectSingleNode("PV").InnerText) Then
                        If kida.ElementXML.SelectSingleNode("PV").InnerText > CSng(kida.ElementHiHI) Then
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "red"
                        Else
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                        End If
                    ElseIf kida.ElementXML.SelectSingleNode("PV").InnerText = "ERR" Then
                        kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "red"
                    End If

                ElseIf sType = "rec_analog" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        '   Call elementTest(element, myP.Value, sType, sEx)
                        Dim myrec As Rectangle
                        Dim rtemp As Double
                        Try
                            myrec = LogicalTreeHelper.FindLogicalNode(mycanvas, kida.ElementName & "_b")
                            rtemp = myrec.Height
                        Catch ex As Exception
                            rtemp = 49
                        End Try

                        If myP.Value <= 0 Then
                            kida.ElementObject.Height = rtemp
                        ElseIf myP.Value >= 100 Then
                            kida.ElementObject.Height = 0
                        Else
                            kida.ElementObject.Height = (1 - myP.Value / 100) * rtemp
                        End If

                        kida.ElementXML = element
                    Else
                        kida.ElementObject.Height = 0
                    End If
                ElseIf sType = "alarm" Or sType = "alarm_plm" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        If myP.Value = "alarm" Then
                            '          kida.ElementXML.SelectSingleNode("flag_flash").InnerText = "yes"
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "red"
                        Else
                            '           kida.ElementXML.SelectSingleNode("flag_flash").InnerText = "no"
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "lawngreen"
                        End If

                        element = kida.ElementXML.Clone
                        Call elementTest(kida, element, myP.Value, sType, sEx)
                        kida.ElementXML = element
                    End If
                ElseIf sType = "calc" Then
                    '  Call Subs(kida.ElementName)
                ElseIf sType = "plm" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(kida, element, myP.Value, sType, sEx)
                        kida.ElementXML = element
                    End If
                ElseIf sType = "string" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(kida, element, myP.Value, sType, sEx)
                        kida.ElementXML = element
                    End If
                ElseIf sType = "status" Then
                    If InStr(kida.ElementTag, ";") > 0 Then
                        atemp = Split(kida.ElementTag, ";")
                        sTag1 = atemp(0) & ".cursta"
                        sTag2 = atemp(1) & ".cursta"
                    Else
                        sTag1 = kida.ElementTag & ".cursta"
                    End If

                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(kida, element, myP.Value, "status", sEx)
                        kida.ElementXML = element
                    Else
                        kida.ElementXML.SelectSingleNode("cursta").InnerText = "error"
                    End If
                    '  kida.ElementObject.DataContext = Nothing
                ElseIf sType = "status_analog" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(kida, element, myP.Value, "status_analog", sEx)
                        kida.ElementXML = element
                    End If

                End If

            Catch ex As Exception
                ncount = 0
            End Try

        End If
        kida.ElementObject.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, New SubPrimeDelegate(AddressOf ThreadStartTimer_kid), kida)

    End Sub
    Private Sub processEqu(ByRef kida As testpu)
        Dim blnBadValue As Boolean = False
        Dim atemp() As String
        Dim atemp1() As String
        Dim rtemp1 As Double = 1
        Dim stemp As String
        Dim sQue As String

        Dim rtemp(10) As Double

        Try
            sQue = kida.ElementEqu
            atemp = Split(kida.ElementTag, ";")
            atemp1 = Split(kida.ElementEqu, ";")

            If atemp.Count >= 1 Then
                For i = 1 To atemp.Count
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, atemp(i - 1), sinusoid) Then
                        If sinusoid.CurrentValue().IsGood Then
                            rtemp(i - 1) = sinusoid.CurrentValue().Value
                        Else
                            blnBadValue = True
                        End If
                    ElseIf IsNumeric(atemp(i - 1)) Then
                        Try
                            rtemp(i - 1) = CDbl(atemp(i - 1))
                        Catch ex As Exception
                            rtemp(i - 1) = 1
                        End Try
                    Else
                        blnBadValue = True
                    End If
                Next
                If blnBadValue Then
                    kida.ElementXML.SelectSingleNode("PV").InnerText = "ERR"
                Else
                    stemp = ""
                    Dim rtemp2, rtemp3 As Double

                    For i = 1 To atemp.Count
                        rtemp3 = CDbl(FormatNumber(rtemp(i - 1), 3))
                        sQue = Replace(sQue, "X" & i, rtemp3)
                    Next

                    Dim parser As New System.Parsers.MQ


                    rtemp2 = parser.Calculate(sQue)

                    kida.ElementXML.SelectSingleNode("PV").InnerText = FormatNumber(rtemp2, 1)

                End If

            End If
        Catch ex As Exception
            kida.ElementXML.SelectSingleNode("PV").InnerText = "ERR"
        End Try
    End Sub
    Private Sub btnOpenComp1_Click(sender As Object, e As RoutedEventArgs) Handles btnOpenComp1.Click
        Dim neww As New TEGCompressor1
        neww.Show()
    End Sub

    Private Sub btnOpenComp2_Click(sender As Object, e As RoutedEventArgs) Handles btnOpenComp2.Click
        Dim neww As New TEGCompressor2
        neww.Show()
    End Sub

    Private Sub btnOpenComp3_Click(sender As Object, e As RoutedEventArgs) Handles btnOpenComp3.Click
        Dim neww As New TEGCompressor3
        neww.Show()
    End Sub

    Private Sub UpdateLiveChart(ByRef chaName As String)
        ' get series names

        Dim ser As DataSeries
        Dim i1 As Integer = 0

        Dim dp As DataPoint
        Dim cha As Chart

        Dim results As OSIsoft.AF.AFListResults(Of OSIsoft.AF.PI.PIPoint, OSIsoft.AF.Asset.AFValue)

        cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)

        results = pts.CurrentValue

        For Each ser In cha.Series

            '     ReDim Preserve sTags(i1)
            '     sTags(i1) = ser.Name
            '   If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, ser.Name, sinusoid) Then
            'avalue = sinusoid.CurrentValue
            If results(i1).IsGood Then
                ' add new data point
                dp = New DataPoint
                dp.XValue = results(i1).Timestamp.LocalTime
                dp.YValue = results(i1).Value '* rTags(i1)
                ser.DataPoints.Add(dp)

                'remove last
                ser.DataPoints.RemoveAt(0)
                i1 = i1 + 1
                ' End If
            End If
        Next
        '   cha.AxesY(0).AxisMinimum = rmin * 0.9
        cha.AxesY(0).AxisMaximum = 400000
        cha.AxesY(0).Interval = 50000
    End Sub
    Private Sub btnDehydration1_Click(sender As Object, e As RoutedEventArgs) Handles btnDehydration1.Click
        Dim newWinThread As New Thread(AddressOf startDehydration)
        iSulfurRemoval = 1
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub
    Private Sub btnDehydration2_Click(sender As Object, e As RoutedEventArgs) Handles btnDehydration2.Click
        Dim newWinThread As New Thread(AddressOf startDehydration)
        iSulfurRemoval = 2
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub
    Private Sub btnDehydration3_Click(sender As Object, e As RoutedEventArgs) Handles btnDehydration3.Click
        Dim newWinThread As New Thread(AddressOf startDehydration)
        iSulfurRemoval = 3
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub
    Public Sub startDehydration()
        Call writeToLog("Dehydration" & iSulfurRemoval)
        If iSulfurRemoval = 1 Then
            Dim newdia As New Dehy_1
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        ElseIf iSulfurRemoval = 2 Then
            Dim newdia As New Dehy_2
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        Else
            Dim newdia As New Dehy_3
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        End If

    End Sub
    Private Sub btnSulfurRemoval1_Click(sender As Object, e As RoutedEventArgs) Handles btnSulfurRemoval1.Click
        Dim newWinThread As New Thread(AddressOf startSulfurRemoval)
        iSulfurRemoval = 1
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()

    End Sub
    Public Sub startSulfurRemoval()
        Call writeToLog("SulfurRemoval" & iSulfurRemoval)
        If iSulfurRemoval = 1 Then
            Dim newdia As New SulfurRemoval1
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        ElseIf iSulfurRemoval = 2 Then
            Dim newdia As New SulfurRemoval2
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        Else
            Dim newdia As New SulfurRemoval3
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        End If

    End Sub

    Private Sub btnSulfurRemoval2_Click(sender As Object, e As RoutedEventArgs) Handles btnSulfurRemoval2.Click
        Dim newWinThread As New Thread(AddressOf startSulfurRemoval)
        iSulfurRemoval = 2
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnSulfurRemoval3_Click(sender As Object, e As RoutedEventArgs) Handles btnSulfurRemoval3.Click
        Dim newWinThread As New Thread(AddressOf startSulfurRemoval)
        iSulfurRemoval = 3
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnSulfurRecovery1_Click(sender As Object, e As RoutedEventArgs) Handles btnSulfurRecovery1.Click
        Dim newWinThread As New Thread(AddressOf startSulfurRecovery)
        iSulfurRemoval = 1
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub
    Public Sub startSulfurRecovery()
        Call writeToLog("SulfurRecovery" & iSulfurRemoval)
        If iSulfurRemoval = 1 Then
            Dim newdia As New Claus1
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        ElseIf iSulfurRemoval = 2 Then
            Dim newdia As New Claus2
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        Else
            Dim newdia As New Claus3
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        End If

    End Sub
    Public Sub startScotStack()

        Call writeToLog("Stack_" & iScotStack)
        If iScotStack = 1 Then
            Dim newdia As New Scot_stack1
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        ElseIf iScotStack = 2 Then
            Dim newdia As New Scot_stack2
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        Else
            Dim newdia As New Scot_stack3
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        End If

    End Sub
    Public Sub startScot()

        Call writeToLog("Scot_" & iScot)
        If iScot = 1 Then
            Dim newdia As New Scot_1
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        ElseIf iScot = 2 Then
            Dim newdia As New Scot_2
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        Else
            Dim newdia As New Scot_3
            newdia.Show()
            System.Windows.Threading.Dispatcher.Run()
        End If

    End Sub
    Public Sub startSulfurInventory()
        Call writeToLog("SulfurDelivery")
        Dim newdia As New SulfurDelivery
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub
    Public Sub startWellPadA()

        Dim newdia As New WellPadA
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub
    Public Sub startWellPadC()

        Dim newdia As New WellPadC
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub

    Private Sub btnSulfurRecovery2_Click(sender As Object, e As RoutedEventArgs) Handles btnSulfurRecovery2.Click
        Dim newWinThread As New Thread(AddressOf startSulfurRecovery)
        iSulfurRemoval = 2
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnSulfurRecovery3_Click(sender As Object, e As RoutedEventArgs) Handles btnSulfurRecovery3.Click
        Dim newWinThread As New Thread(AddressOf startSulfurRecovery)
        iSulfurRemoval = 3
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnWellPadA_Click(sender As Object, e As RoutedEventArgs) Handles btnWellPadA.Click
        Dim newWinThread As New Thread(AddressOf startWellPadA)
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnWellPadC_Click(sender As Object, e As RoutedEventArgs) Handles btnWellPadC.Click
        Dim newWinThread As New Thread(AddressOf startWellPadC)
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub elementTest(ByRef kida As testpu, ByRef element As Xml.XmlNode, ByVal curval As Object, ByVal sType As String, ByVal sEx As String)
        'Dim element As Xml.XmlNode = aak1.ElementXML.Clone
        'Dim sEx As String
        'modified by yutecxa 2022-8-5 add condition for txtTank1_inv
        'If curval.GetType.Name = "Single" Or curval.GetType.Name = "Int16" Or curval.GetType.Name = "Int32" Or curval.GetType.Name = "AFEnumerationValue" Then
        If element.Attributes.GetNamedItem("name").Value = "txtTank1_inv" Or curval.GetType.Name = "Single" Or curval.GetType.Name = "Int16" Or curval.GetType.Name = "Int32" Or curval.GetType.Name = "AFEnumerationValue" Then
            If sType = "analog" Or sType = "batmtr" Or sType = "plm" Or sType = "tank" Then
                Try
                    If element.SelectSingleNode(sEx).InnerText <> CStr(curval) Then
                        element.SelectSingleNode(sEx).InnerText = CStr(curval)
                        ' check for limits
                        element.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                        If kida.ElementHiHI <> "none" And IsNumeric(kida.ElementHiHI) Then
                            If curval > CSng(kida.ElementHiHI) Then
                                element.SelectSingleNode("flag_fg").InnerText = "red"
                            Else
                                '       element.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                            End If
                        End If
                    End If


                    ''''Can be deleted for displaying original data
                    If kida.ElementPITag = "GSB_GROSS_EXPORT_GAS_RATE_CALC.PV" Then

                        If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, "FI020401.DACA.PV", sinusoid) Then

                            'Corrected on 2022/4/14 
                            element.SelectSingleNode(sEx).InnerText = CStr(sinusoid.CurrentValue().Value + 817)

                        End If
                    End If
                    ''''Can be deleted for displaying original data




                    If kida.ElementLoLo <> "none" And IsNumeric(kida.ElementLoLo) Then
                        If curval < CSng(kida.ElementLoLo) Then
                            element.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                        Else
                            '  element.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                        End If
                    End If

                Catch ex As Exception
                    ' element.SelectSingleNode(sEx).InnerText = "N/A"
                End Try
            ElseIf sType = "tank" Then
                Try
                    If element.SelectSingleNode(sEx).InnerText <> CStr(curval) Then
                        element.SelectSingleNode(sEx).InnerText = CStr(curval)
                        ' check for limits

                    End If
                Catch ex As Exception

                End Try
            ElseIf sType = "alarm_plm" Then
                Try
                    If element.SelectSingleNode(sEx).InnerText <> CStr(curval) Then
                        element.SelectSingleNode(sEx).InnerText = CStr(curval)
                        ' check for limits

                    End If
                Catch ex As Exception

                End Try
            ElseIf sType = "status" Then
                '  curval = CInt(Math.Ceiling(Rnd() * 3))
                ' sEx = element.Attributes.GetNamedItem("ext").Value
                If sEx = "flag_msgtxt" Or sEx = "msgColor" Then
                    ' Call getMessage(element, curval, sEx)
                Else
                    If curval.GetType.Name = "AFEnumerationValue" Then
                        If curval.ToString = "CLOSE" Or curval.ToString = "CLOSED" Then
                            element.SelectSingleNode("cursta").InnerText = "closed"
                        ElseIf curval.ToString = "OPEN" Or curval.ToString = "OPENED" Then
                            element.SelectSingleNode("cursta").InnerText = "open"
                        ElseIf curval.ToString = "STOP" Or curval.ToString = "STOPPED" Then
                            element.SelectSingleNode("cursta").InnerText = "transit"
                        ElseIf curval.ToString = "ON" Then
                            element.SelectSingleNode("cursta").InnerText = "on"
                        ElseIf curval.ToString = "OFF" Then
                            element.SelectSingleNode("cursta").InnerText = "off"
                        ElseIf curval.ToString = "Inbet" Then
                            element.SelectSingleNode("cursta").InnerText = "error"
                        Else
                            element.SelectSingleNode("cursta").InnerText = "closed"
                        End If
                    Else

                        If CSng(curval) = 1 Then
                            element.SelectSingleNode("cursta").InnerText = "open"
                        ElseIf CSng(curval) = 2 Then
                            element.SelectSingleNode("cursta").InnerText = "closed"
                        ElseIf CSng(curval) = 3 Then
                            element.SelectSingleNode("cursta").InnerText = "transit"
                        ElseIf CSng(curval) = 4 Then
                            element.SelectSingleNode("cursta").InnerText = "transit"
                        ElseIf CSng(curval) = 0 Then
                            element.SelectSingleNode("cursta").InnerText = "error"
                        ElseIf CSng(curval) = 9 Then
                            element.SelectSingleNode("cursta").InnerText = "open"
                        ElseIf CSng(curval) = 10 Then
                            element.SelectSingleNode("cursta").InnerText = "closed"
                        ElseIf CSng(curval) = 14 Then
                            element.SelectSingleNode("cursta").InnerText = "locked"
                        End If
                    End If

                End If
            ElseIf sType = "status_analog" Then
                If CSng(curval) > 0 Then
                    element.SelectSingleNode("cursta").InnerText = "on"
                Else
                    element.SelectSingleNode("cursta").InnerText = "off"
                End If
            End If
        ElseIf curval.GetType.Name = "String" Then
            If sType = "alarm" Then
                element.SelectSingleNode(sEx).InnerText = CStr(curval)
            Else
                element.SelectSingleNode(sEx).InnerText = CStr(curval)
            End If

        Else
            element.SelectSingleNode(sEx).InnerText = curval.name
        End If

        ' aak1.ElementXML = element
        'aak1.ElementObject.DataContext = element
    End Sub




    Private Sub btnInventory_Click(sender As Object, e As RoutedEventArgs) Handles btnInventory.Click
        Dim newWinThread As New Thread(AddressOf startSulfurInventory)
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnScot_stack1_Click(sender As Object, e As RoutedEventArgs) Handles btnScot_stack1.Click
        Dim newWinThread As New Thread(AddressOf startScotStack)
        iScotStack = 1
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnScot_stack2_Click(sender As Object, e As RoutedEventArgs) Handles btnScot_stack2.Click
        Dim newWinThread As New Thread(AddressOf startScotStack)
        iScotStack = 2
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnScot_stack3_Click(sender As Object, e As RoutedEventArgs) Handles btnScot_stack3.Click
        Dim newWinThread As New Thread(AddressOf startScotStack)
        iScotStack = 3
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub



    Private Sub btnScot1_Click(sender As Object, e As RoutedEventArgs) Handles btnScot1.Click
        Dim newWinThread As New Thread(AddressOf startScot)
        iScot = 1
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnScot2_Click(sender As Object, e As RoutedEventArgs) Handles btnScot2.Click
        Dim newWinThread As New Thread(AddressOf startScot)
        iScot = 2
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub btnScot3_Click(sender As Object, e As RoutedEventArgs) Handles btnScot3.Click
        Dim newWinThread As New Thread(AddressOf startScot)
        iScot = 3
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Private Sub getTrend(ByVal sender As TextBox, ByVal e As System.Windows.RoutedEventArgs)
        Dim sEX As String
        Dim sScale As String
        Dim sQue As String
        Dim sVar As String
        Dim sType As String
        Dim sHiHi, sLoLo As String

        If My.Computer.Keyboard.CtrlKeyDown Then

            sTagDrag = sender.Tag
            sEX = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("ext").Value
            Try
                sScale = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("scale").Value
            Catch ex As Exception
                sScale = "1"
            End Try

            strXML = "<variables>"
            strXML = strXML & "<input trend='yes' var='" & sender.Tag & "' ext='" & sEX & "' scale='" & sScale & "'><value/><time/></input>"
            strXML = strXML & "</variables>"

            Clipboard.SetText(strXML)

        Else
            If sender.Tag <> "" Then
                ' find sex
                sEX = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("ext").Value
                sVar = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("var").Value
                sType = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("type").Value

                Try
                    sHiHi = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("hihi").Value
                Catch ex As Exception
                    sHiHi = "none"
                End Try

                Try
                    sLoLo = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("lolo").Value
                Catch ex As Exception
                    sLoLo = "none"
                End Try

                Try
                    sScale = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("scale").Value
                Catch ex As Exception
                    sScale = "1"
                End Try

                Try
                    sQue = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("equ").Value
                Catch ex As Exception
                    sQue = ""
                End Try

                strXML = "<variables>"
                strXML = strXML & "<input trend='yes' hihi='" & sHiHi & "' lolo='" & sLoLo & "' tag='" & sender.Tag & "' type='" & sType & "' var='" & sVar & "' equ='" & sQue & "' ext='" & sEX & "' scale='" & sScale & "'><value/><time/></input>"
                strXML = strXML & "</variables>"

                Dim newWinThread As New Thread(AddressOf startADDHOC)
                'strXML = sender.Tag
                newWinThread.IsBackground = True
                newWinThread.SetApartmentState(ApartmentState.STA)
                newWinThread.Start()
            End If
        End If


    End Sub
    Private Sub sendEmail()
        Dim xlApp As New Outlook.Application
        Dim OutlookMessage As Outlook.MailItem
        Dim atemp() As String
        Dim sTemp As String

        Try
            OutlookMessage = xlApp.CreateItem(Outlook.OlItemType.olMailItem)

            sTemp = sUser & "@chevron.com"
            Dim fileReader As String
            fileReader = My.Computer.FileSystem.ReadAllText("c:\Projects\SCRIBE\config\ops.html")

            With OutlookMessage
                .To = sTemp '"bzye@chevron.com"
                .CC = ""
                .BCC = ""
                .Subject = "CDB Operations Data - do not reply"
                .HTMLBody = fileReader
                '.Attachments.Add TempFilePath & TempFileName
                'Display()
                .Send()
            End With

        Catch ex As Exception
            MessageBox.Show("Mail could Not be sent") 'if you dont want this message, simply delete this line  
        Finally
            OutlookMessage = Nothing
            xlApp = Nothing

        End Try
    End Sub
    Private Sub btnSendEmail_Click(sender As Object, e As RoutedEventArgs) Handles btnSendEmail.Click

        btnScot_stack1.Visibility = Visibility.Hidden
        btnScot_stack2.Visibility = Visibility.Hidden
        btnScot_stack3.Visibility = Visibility.Hidden

        btnScot1.Visibility = Visibility.Hidden
        btnScot2.Visibility = Visibility.Hidden
        btnScot3.Visibility = Visibility.Hidden

        btnSulfurRemoval1.Visibility = Visibility.Hidden
        btnSulfurRemoval2.Visibility = Visibility.Hidden
        btnSulfurRemoval3.Visibility = Visibility.Hidden

        btnSulfurRecovery1.Visibility = Visibility.Hidden
        btnSulfurRecovery2.Visibility = Visibility.Hidden
        btnSulfurRecovery3.Visibility = Visibility.Hidden

        txtChartTimer.Visibility = Visibility.Visible

        Dim xi As Double = (System.Windows.SystemParameters.PrimaryScreenWidth) / mycanvas.Width
        Dim yi As Double = (System.Windows.SystemParameters.PrimaryScreenHeight) / mycanvas.Height


        Dim bounds As Rect = VisualTreeHelper.GetDescendantBounds(ops1)

        ops1.Measure(New Size(ops1.Width, ops1.Height))

        ops1.Arrange(New Rect(New Size(ops1.Width, ops1.Height)))

        Dim renderBitmap As RenderTargetBitmap
        ' renderBitmap = New RenderTargetBitmap(cha.Width * 1.2, cha.Height * 1.2, 96D, 96D, PixelFormats.Pbgra32)
        renderBitmap = New RenderTargetBitmap(mycanvas.Width, mycanvas.Height, 96D, 96D, PixelFormats.Pbgra32)
        renderBitmap.Render(mycanvas)


        Dim crop As New CroppedBitmap(renderBitmap, New Int32Rect(Canvas.GetLeft(ops1), Canvas.GetTop(ops1), ops1.Width, ops1.Height))
        Dim outStream As FileStream = New FileStream("c:\Projects\SCRIBE\ops1.png", FileMode.OpenOrCreate)
        Dim encoder As New PngBitmapEncoder
        encoder.Frames.Add(BitmapFrame.Create(crop))

        encoder.Save(outStream)
        outStream.Close()
        encoder = Nothing

        'Height = "283" Canvas.Left="1309" Stroke="Black" Canvas.Top="338" Width="529"

        Dim encoder1 As New PngBitmapEncoder
        Dim crop1 As New CroppedBitmap(renderBitmap, New Int32Rect(xi * 1309, yi * 338, xi * 529, yi * 283))
        encoder1.Frames.Add(BitmapFrame.Create(crop1))

        Dim outStream1 As New FileStream("c:\Projects\SCRIBE\ops2.png", FileMode.OpenOrCreate)
        encoder1.Save(outStream1)
        outStream1.Close()
        encoder1 = Nothing

        'Height="406" Canvas.Left="1309" Stroke="Black" Canvas.Top="629" Width="529"

        Dim encoder2 As New PngBitmapEncoder
        Dim crop2 As New CroppedBitmap(renderBitmap, New Int32Rect(xi * 1309, yi * 629, xi * 529, yi * 406))
        encoder2.Frames.Add(BitmapFrame.Create(crop2))

        Dim outStream2 As New FileStream("c:\Projects\SCRIBE\ops3.png", FileMode.OpenOrCreate)
        encoder2.Save(outStream2)
        outStream2.Close()
        encoder2 = Nothing



        'Height="496" Canvas.Left="640" Stroke="Black" StrokeThickness="2" Canvas.Top="384" Width="625"
        Dim encoder3 As New PngBitmapEncoder
        Dim crop3 As New CroppedBitmap(renderBitmap, New Int32Rect(xi * 640, yi * 384, xi * 625, yi * 496))
        encoder3.Frames.Add(BitmapFrame.Create(crop3))

        Dim outStream3 As New FileStream("c:\Projects\SCRIBE\ops4.png", FileMode.OpenOrCreate)
        encoder3.Save(outStream3)
        outStream3.Close()
        encoder3 = Nothing

        btnScot_stack1.Visibility = Visibility.Visible
        btnScot_stack2.Visibility = Visibility.Visible
        btnScot_stack3.Visibility = Visibility.Visible

        btnScot1.Visibility = Visibility.Visible
        btnScot2.Visibility = Visibility.Visible
        btnScot3.Visibility = Visibility.Visible

        btnSulfurRemoval1.Visibility = Visibility.Visible
        btnSulfurRemoval2.Visibility = Visibility.Visible
        btnSulfurRemoval3.Visibility = Visibility.Visible

        btnSulfurRecovery1.Visibility = Visibility.Visible
        btnSulfurRecovery2.Visibility = Visibility.Visible
        btnSulfurRecovery3.Visibility = Visibility.Visible

        txtChartTimer.Visibility = Visibility.Hidden

        '  Call sendEmail()
    End Sub

    Private Sub getTrendStatus(ByVal sender As Image, ByVal e As System.Windows.RoutedEventArgs)
        Dim sEX As String

        If My.Computer.Keyboard.CtrlKeyDown Then

            sTagDrag = sender.Tag
            sEX = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("ext").Value

            strXML = "<variables>"
            strXML = strXML & "<input trend='yes' var='" & sender.Tag & "' ext='" & sEX & "'><value/><time/></input>"
            strXML = strXML & "</variables>"

            Clipboard.SetText(strXML)

        Else


            If sender.Tag <> "" Then
                ' find sex
                sEX = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("ext").Value

                strXML = "<variables>"
                strXML = strXML & "<input trend='yes' var='" & sender.Tag & "' ext='" & sEX & "'><value/><time/></input>"
                strXML = strXML & "</variables>"

                Dim newWinThread As New Thread(AddressOf startADDHOC)
                'strXML = sender.Tag
                newWinThread.IsBackground = True
                newWinThread.SetApartmentState(ApartmentState.STA)
                newWinThread.Start()
            End If
        End If
    End Sub
    Public Sub startADDHOC()
        Dim newdia As New ADDHOC(strXML, sServerName)
        ' newdia.xmlfile = "Operations_map.txt"
        newdia.xmlfile = ""
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub
    Sub newelem_PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        '  Dim sTag, sName As String
        '  Dim myobj As Object

        'MessageBox.Show(e.PropertyName + " was changed: ")
        If e.PropertyName = "ElementXML" Then
            ' If sender.ElementValue = "none" Then
            'sender.elementtag = "none"
            'Else
            '  sName = sender.ElementName
            ' myobj = LogicalTreeHelper.FindLogicalNode(mycanvas, sName)
            Try
                sender.elementobject.datacontext = sender.ElementXML
            Catch ex As Exception

            End Try

            'End If
        End If
    End Sub

    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        If UCase(sUser) <> "PCNNMS1" Then  'PCN serice acount(PCNNMS1)
            If Now > sAppTime Then  'PCN serice acount(PCNNMS1)
                Me.Close()
            End If
        End If
    End Sub

    Private Sub timer2_Tick(sender As Object, e As EventArgs) Handles timer2.Tick


        txtOE.Text = sMessages(iMessage)
        iMessage = iMessage + 1
        If iMessage > 8 Then
            iMessage = 0
        End If

    End Sub
    Public Sub Quad0(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = 0
        Me.Left = 0
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        For Each kida In elementList
            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "#FF000042"
        Next
    End Sub

    Public Sub Quad1(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = 0
        Me.Left = SystemParameters.PrimaryScreenWidth / 2
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Public Sub Quad2(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = (SystemParameters.WorkArea.Height) / 2
        Me.Left = 0
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Public Sub Quad3(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = (SystemParameters.WorkArea.Height) / 2
        Me.Left = SystemParameters.PrimaryScreenWidth / 2
        Me.Width = SystemParameters.PrimaryScreenWidth / 2
        Me.Height = (SystemParameters.WorkArea.Height) / 2 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Public Sub Quad00(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Top = 0
        Me.Left = 0
        Me.Width = SystemParameters.PrimaryScreenWidth
        Me.Height = SystemParameters.WorkArea.Height 'SystemParameters.PrimaryScreenHeight ' - 30 - 30    End Sub
    End Sub
    Private Sub MainWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - SystemParameters.WindowCaptionHeight) / (mycanvas.Height) ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale

        '   scrollViewer.Height = System.Windows.SystemParameters.WorkArea.Height
        '  scrollViewer.Width = System.Windows.SystemParameters.WorkArea.Width
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        Me.Height = System.Windows.SystemParameters.WorkArea.Height * 0.9 'SystemParameters.PrimaryScreenHeight * 0.7 '960 '1706
        Me.Width = SystemParameters.PrimaryScreenWidth * 0.9 '1706 '1016

        Me.Left = 0
        Me.Top = 0

        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - SystemParameters.WindowCaptionHeight) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale

        ' scrollViewer.Height = Me.Height
        ' scrollViewer.Width = Me.Width
    End Sub

    '  Private Sub MainWindow_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
    '  Try
    '  If e.ChangedButton = MouseButton.Left Then
    ' Me.DragMove()
    ' End If
    ' Catch ex As Exception

    'End Try
    '  End Sub

    'Private Sub timer3_Tick(sender As Object, e As EventArgs) Handles timer3.Tick
    '    '   mytxt = DirectCast(kida.ElementObject, TextBox)
    '    ' Dim itemp As Integer = (CDate("10/1/2019") - Now).TotalDays
    '    Dim tspan As TimeSpan
    '    Dim iValue As Integer
    '    Dim rValue As Single
    '    Dim stemp As String = ""
    '    Dim sValue As String = ""
    '    timer3.Interval = TimeSpan.FromSeconds(5)
    '    Try
    '        tspan = CDate("10/1/2019") - Now
    '        Call getWhole(CStr(tspan.TotalDays), iValue, sValue, rValue)

    '        stemp = iValue & " days " & " "

    '        Call getWhole(rValue * 24, iValue, sValue, rValue)
    '        stemp = stemp & sValue & " hr "

    '        Call getWhole(rValue * 60, iValue, sValue, rValue)
    '        stemp = stemp & sValue & " min "

    '        Call getWhole(rValue * 60, iValue, sValue, rValue)
    '        stemp = stemp & sValue & " sec"

    '        lblCountdown.Text = stemp

    '    Catch ex As Exception

    '    End Try

    'End Sub

End Class
