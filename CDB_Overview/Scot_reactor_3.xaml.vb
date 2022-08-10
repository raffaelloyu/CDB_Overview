Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports System.Threading

Imports Visifire.Charts
Imports OSIsoft.AF
Imports OSIsoft.AF.PI
Imports OSIsoft.AF.Asset

Imports System.IO
Imports System.ComponentModel

Imports OSIsoft.AF.Time
Public Class Scot_reactor_3
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

    Private config_aa As New Xml.XmlDocument
    Private sPageType As String
    Private strXML As String

    Private sLineColors(16) As Brush
    Private foundPoints, foundPoints1, foundPointsld As IEnumerable(Of OSIsoft.AF.PI.PIPoint)
    Private pts(3), pts1, pts_ld As OSIsoft.AF.PI.PIPointList
    Private WithEvents timer1 As New DispatcherTimer

    Private sTime_start, sTime_end As Date
    Private times() As AFTime


    Private rValues_as(6, 0) As Double
    Private sDates_as(6, 0) As String
    Private iPoints_as(6) As String

    Private sDates(0) As String
    Private blnAddMarkers, blnAddLabels, blnAddChart As Boolean
    Private pi_col_xls_tags() As String

    Private oWeelAuto1 As New DoubleAnimation
    Private oWeelAuto0 As New DoubleAnimation
    Private oWeelAuto2 As New DoubleAnimation

    Private oTransform1 As New RotateTransform
    Private oTransform0 As New RotateTransform

    Private blnStop As Boolean = False
    Private iops As Integer

    Private sValueFormatString As String
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ' Add any initialization after the InitializeComponent() call.
        oWeelAuto1.From = 0
        oWeelAuto1.From = 360

        oWeelAuto1.Duration = New Duration(New TimeSpan(0, 0, 0, 5))
        oWeelAuto1.RepeatBehavior = New RepeatBehavior()

        oWeelAuto1.RepeatBehavior = RepeatBehavior.Forever

        oWeelAuto0.From = 0
        oWeelAuto0.From = 360

        oWeelAuto0.Duration = New Duration(New TimeSpan(0, 0, 0, 5))
        oWeelAuto0.RepeatBehavior = New RepeatBehavior(0)

        oWeelAuto2.From = 1
        oWeelAuto2.From = 0

        oWeelAuto2.Duration = New Duration(New TimeSpan(0, 0, 0, 2))
        oWeelAuto2.RepeatBehavior = New RepeatBehavior()
        oWeelAuto2.RepeatBehavior = RepeatBehavior.Forever

        sUser = Environment.UserName


        PIServer = piservers("pnwpappv003")
        srvAF = OSIsoft.AF.PI.PIServer.FindPIServer("pnwpappv003")
        sServerName = "pnwpappv003"

        sLineColors(0) = Brushes.Violet 'Violat
        sLineColors(1) = Brushes.CornflowerBlue
        sLineColors(2) = Brushes.OliveDrab
        sLineColors(3) = Brushes.DarkOrange
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

        sPageType = "Scot_reactor_3"
        '
        Dim inum As Integer
        inum = 510

        ReDim rValues_as(8, inum)
        ReDim sDates_as(8, inum)
        ReDim iPoints_as(8)

        Call createPage("", "", sPageType)
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

        '   filename = AppDomain.CurrentDomain.BaseDirectory & "XAML\" & sPageType & ".xaml"
        config_aa.Load(AppDomain.CurrentDomain.BaseDirectory & "XML\" & sPageType & "_map.txt")

        elementList.Clear()
        chartList.Clear()
        root = mycanvas
        '   myFrame.Content = mycanvas
        'Height="1098" Width="1880"
        'Height="720" Width="1160" 

        mycanvas.Width = 1030
        mycanvas.Height = 660
        'Height="910" Width="1682"
        'Height="778" Width="1682"
        ' Height="710" Width="1160"

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
                        sTag = sTagName & "." & sEX
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
                        End If
                        newelem = testpu.CreateNewElement
                        AddHandler newelem.PropertyChanged, AddressOf newelem_PropertyChanged
                        elementList.Add(newelem)
                        newelem.ElementName = sName
                        newelem.ElementTag = sTagName

                        stemp = config_aa.SelectSingleNode("variables/input[@name='" & sName & "']").Attributes.GetNamedItem("type").Value

                        If stemp = "animation" Then
                            newimage.RenderTransform = oTransform0
                            'newimage.RenderTransform.Freeze()
                        End If

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

        blnStop = True

        For Each kid In elementList
            Try
                Dispatcher.Invoke(DispatcherPriority.Background, TimeSpan.FromSeconds(3), New SubPrimeDelegate(AddressOf ThreadStartTimer_kid), kid)
            Catch ex As Exception
                kid = Nothing
            End Try
        Next



        If sPageType = "Scot_reactor_3" Then
            Call createReactor()
            Call createBarChart()
        End If

    End Sub
    Private Sub createReactor()
        Dim sTags(5), sTagDesc(5) As String
        Dim avalues As AFValues
        Dim timerange1 As AFTimeRange
        Dim returnValue As IDictionary(Of Data.AFSummaryTypes, AFValue)
        Dim mylbl As Label

        sTime_end = Now
        sTime_start = DateAdd(DateInterval.Hour, -24, Now)

        timerange1 = New AFTimeRange(sTime_start, sTime_end, Globalization.CultureInfo.CurrentCulture)

        ReDim sTags(2)

        sTags(0) = "XHN_GAS-PLNT_TRN-3_SCOT-REACTR_DP.PV"
        sTags(1) = "XHN_GAS-PLNT_TRN-3_SCOT-REACTR_GAS-OUTLET-T.PV"
        sTags(2) = "TIC070902_3.DACA.PV"

        foundPoints = OSIsoft.AF.PI.PIPoint.FindPIPoints(srvAF, sTags)
        pts(1) = New OSIsoft.AF.PI.PIPointList(foundPoints)

        ReDim pi_col_xls_tags(3)

        pi_col_xls_tags(1) = "XHN_GAS-PLNT_TRN-3_SCOT-REACTR_DP.PV"
        pi_col_xls_tags(2) = "XHN_GAS-PLNT_TRN-3_SCOT-REACTR_GAS-OUTLET-T.PV"
        pi_col_xls_tags(3) = "TIC070902_3.DACA.PV"

        iops = 3
        Call refreshChart("chart_flows_in", 1)

    End Sub
    Private Sub callMultiCharts(sender As Object, e As RoutedEventArgs)
        Dim stemp As String = sender.tag
        callMultiChart(stemp)
    End Sub
    Private Sub callMultiChart(ByVal stemp As String)
        'Dim stemp As String
        Dim atemp() As String
        Dim sEx, sVar As String
        Dim sTitle As String

        atemp = Split(stemp, ";")
        sTitle = atemp(0)

        strXML = "<variables title='" & sTitle & "'>"
        For i = 1 To UBound(atemp) - 1
            sEx = config_aa.DocumentElement.SelectSingleNode("input[@name='" & atemp(i) & "']").Attributes.GetNamedItem("ext").Value
            sVar = config_aa.DocumentElement.SelectSingleNode("input[@name='" & atemp(i) & "']").Attributes.GetNamedItem("var").Value
            strXML = strXML & "<input trend='yes' var='" & sVar & "' ext='" & sEx & "'><value/><time/></input>"
        Next

        strXML = strXML & "</variables>"

        Dim newWinThread As New Thread(AddressOf startADDHOC)
        'strXML = sender.Tag
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub
    Private Sub createBarChart()
        Dim iPoints As Integer
        '    Dim sChartName As String
        Dim ileft, iTop, iWidth, iHeight As Integer
        Dim nSeries, nSeries_from As Integer
        Dim chatype As RenderAs
        Dim sColors() As Brush
        Dim sSeriesNames(10) As String
        '    Dim iPoints As Integer

        Dim sTYpes(8) As RenderAs
        Dim cha As Chart
        Dim avalue As AFValue
        Dim sUnits(2) As String
        Dim sQue As String
        Dim sTag As String
        Dim atemp() As String
        Dim atemp1() As String
        Dim blnBadValue As Boolean = False
        Dim rtemp() As Double
        Dim sTags(4, 4) As String

        sTags(0, 0) = "rtDt_0_11"
        sTags(0, 1) = "rtDt_0_12"
        sTags(0, 2) = "rtDt_0_13"
        sTags(0, 3) = "rtDt_0_14"

        sTags(1, 0) = "rtDt_11_21"
        sTags(1, 1) = "rtDt_12_22"
        sTags(1, 2) = "rtDt_13_23"
        sTags(1, 3) = "rtDt_14_24"

        sTags(2, 0) = "rtDt_21_31"
        sTags(2, 1) = "rtDt_22_32"
        sTags(2, 2) = "rtDt_23_33"
        sTags(2, 3) = "rtDt_24_34"

        sTags(3, 0) = "rtDt_31_1"
        sTags(3, 1) = "rtDt_32_1"
        sTags(3, 2) = "rtDt_33_1"
        sTags(3, 3) = "rtDt_34_1"

        Dim sTemps(4) As String

        sTemps(1) = "TOP-TIN"
        sTemps(2) = "MIDD-TOP"
        sTemps(3) = "BTM-MIDD"
        sTemps(4) = "TOUT-BTM"

        ReDim rValues_as(4, 5)
        ReDim sDates_as(4, 5)

        For jj = 1 To 4 'Step -1
            '     sTags(jj - 1, 1) = "rtDt_0_1" & jj
            '     sTags(2) = "rtDt_1" & jj & "_2" & jj
            '     sTags(3) = "rtDt_2" & jj & "_3" & jj
            '    sTags(4) = "rtDt_3" & jj & "_1"

            'rtDt_0_12
            'rtDt_12_22
            sSeriesNames(jj) = "Set " & jj
            '  sSeriesNames(2) = "TOP-MIDD"
            ' sSeriesNames(3) = "MIDD-BTM"
            ' sSeriesNames(4) = "BTM-TOUT"


            For ii = 0 To 3 ' Step -1
                sQue = config_aa.SelectSingleNode("variables/input[@name='" & sTags(jj - 1, ii) & "']").Attributes.GetNamedItem("equ").Value
                sTag = config_aa.SelectSingleNode("variables/input[@name='" & sTags(jj - 1, ii) & "']").Attributes.GetNamedItem("var").Value

                Try
                    atemp = Split(sTag, ";")
                    atemp1 = Split(sQue, ";")
                    ReDim rtemp(atemp1.Count)

                    If atemp.Count > 1 Then
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
                        If Not blnBadValue Then
                            Dim rtemp2, rtemp3 As Double

                            For i = 1 To atemp.Count
                                rtemp3 = CDbl(FormatNumber(rtemp(i - 1), 3))
                                sQue = Replace(sQue, "X" & i, rtemp3)
                            Next

                            Dim parser As New System.Parsers.MQ
                            rValues_as(ii, jj - 1) = parser.Calculate(sQue)
                            sDates_as(ii, jj - 1) = sTemps(jj)
                        End If

                    End If
                Catch ex As Exception
                    'kida.ElementXML.SelectSingleNode("PV").InnerText = "ERR"
                End Try

            Next
            iPoints_as(jj - 1) = 4
        Next
        ' Height="434" Canvas.Left="622" Stroke="Gray" Canvas.Top="184" Width="390"
        ileft = 622
        iTop = 196
        iWidth = 390
        iHeight = 454
        sUnits(1) = "kPa"
        sUnits(2) = "Deg C"

        '  sSeriesNames(1) = "SCOT Reactor DP"
        '  sSeriesNames(2) = "SCOT Reactor DT"
        iops = 4

        chatype = RenderAs.Column
        nSeries = iops
        nSeries_from = 0
        ReDim sColors(nSeries)
        '   ReDim sSeriesNames(nSeries)
        ReDim sTYpes(nSeries)
        iPoints = UBound(sDates) '- 2



        For ii1 = 1 To iops
            sTYpes(ii1) = RenderAs.Bar
            sColors(ii1) = sLineColors(ii1) 'New SolidColorBrush(ColorConverter.ConvertFromString("#FF0B335A")) '#FFC6BEC6
            'sSeriesNames(ii1) = pi_col_xls_tags(ii1)
        Next

        blnAddMarkers = False
        blnAddLabels = False
        blnAddChart = True
        sValueFormatString = "MM/dd"

        Call createNewChart(cha, "chart_bar", ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, sUnits, sValueFormatString)
    End Sub
    Private Sub refreshChart(ByVal sChartName As String, ByVal ipt As Integer)
        Dim iPoints As Integer
        Dim sTags(2) As String

        '    Dim sChartName As String
        Dim ileft, iTop, iWidth, iHeight As Integer
        Dim nSeries, nSeries_from As Integer
        Dim chatype As RenderAs
        Dim sColors() As Brush
        Dim sSeriesNames(10) As String
        '    Dim iPoints As Integer

        Dim sTYpes(8) As RenderAs
        Dim cha As Chart
        Dim avalue As AFValue
        Dim sUnits(2) As String

        sTime_end = Now
        sTime_start = DateAdd(DateInterval.Hour, -24, sTime_end)

        ' If False Then

        '   Call getChartData(2, strXML, iops, iPoints, pts)

        sTime_end = Now
        sTime_start = DateAdd(DateInterval.Day, -7, sTime_end)

        ' If False Then
        Call getChartData(0, strXML, iops, iPoints, pts(ipt))

        ' Height="321" Canvas.Left="18" Stroke="Gray" Canvas.Top="306" Width="611"
        ileft = 10
        iTop = 300
        iWidth = 620
        iHeight = 345
        sUnits(1) = "kPa"
        sUnits(2) = "Deg C"

        sSeriesNames(1) = "SCOT Reactor DP"
        sSeriesNames(2) = "SCOT Reactor DT"
        iops = 2

        If sChartName = "chart_flows_lean" Then
            'Height="203" Canvas.Left="459" Canvas.Top="697" Width="426"
            'Height="221" Canvas.Left="988" Stroke="Transparent" Canvas.Top="35" Width="480"

            ileft = 988
            iTop = 35
            iWidth = 480
            iHeight = 221
            sUnits(1) = "deg C"

            sSeriesNames(1) = "Chamber Temp 1"
            sSeriesNames(2) = "Chamber Temp 2"
            '  sSeriesNames(3) = "to SCOT"
        ElseIf sChartName = "chart_flows_acid" Then
            'Height="210" Canvas.Left="1122" Canvas.Top="695" Width="550"

            ileft = 1122
            iTop = 695
            iWidth = 550
            iHeight = 220
            sUnits(1) = "m3/hr"

            sSeriesNames(1) = "To KO Drum"
            sSeriesNames(2) = "to Flash Gas Drum"
        End If

        chatype = RenderAs.Line
        nSeries = iops
        nSeries_from = 0
        ReDim sColors(nSeries)
        '   ReDim sSeriesNames(nSeries)
        ReDim sTYpes(nSeries)
        iPoints = UBound(sDates) '- 2

        For ii1 = 1 To iops
            sTYpes(ii1) = RenderAs.Line
            sColors(ii1) = sLineColors(ii1) 'New SolidColorBrush(ColorConverter.ConvertFromString("#FF0B335A")) '#FFC6BEC6
            'sSeriesNames(ii1) = pi_col_xls_tags(ii1)
        Next

        blnAddMarkers = False
        blnAddLabels = False
        blnAddChart = True
        sValueFormatString = "MM/dd"

        Call createNewChart(cha, sChartName, ileft, iTop,
                            iWidth, iHeight, nSeries, nSeries_from, sSeriesNames,
                            sTYpes, sColors, iPoints, sUnits, sValueFormatString)

    End Sub

    Private Sub createNewChart(ByRef cha As Chart, ByVal chaName As String, ByVal iLeft As Integer, ByVal iTop As Integer,
                               ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal nSeries As Integer, ByVal nSeries_from As Integer,
                               ByVal sSeriesNames() As String,
                               ByVal chatype() As RenderAs, ByVal sLineColor() As Brush, ByVal iPoints As Integer, ByVal sYTitle() As String, ByVal sValueFormatString As String)

        Dim dp As DataPoint
        Dim stemp As String
        Dim itemp As Integer = 0

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
        ''   myXax.ValueFormatString = "MM/dd"

        Dim myaxL As AxisLabels = New AxisLabels
        myaxL.FontColor = Brushes.Black
        myXax.AxisLabels = myaxL

        Dim mygr As New ChartGrid
        mygr.LineStyle = LineStyles.Dashed
        mygr.LineThickness = 0.5
        mygr.LineColor = Brushes.Black
        myXax.Grids.Add(mygr)

        ' myXax.IntervalType = IntervalTypes.Days
        '  myXax.Interval = 7

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
        mygr1.LineColor = Brushes.Black
        myYax.Grids.Add(mygr1)
        myYax.Title = sYTitle(1)
        cha.AxesY.Add(myYax)

        'title
        Dim myTitle As New Title
        cha.Titles.Add(myTitle)

        ' secondary Y
        Dim myYax1 As New Axis
        myYax1.AxisType = AxisTypes.Secondary
        cha.AxesY.Add(myYax1)
        myYax1.Title = sYTitle(2)
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
            ser.LightingEnabled = False



            If blnAddMarkers Then
                ser.MarkerEnabled = True
            End If

            If blnAddLabels Then
                ser.LabelEnabled = True
            End If

            If ser.RenderAs = RenderAs.Line Or ser.RenderAs = RenderAs.Spline Then
                '     ser.LabelEnabled = True
                myXax.IntervalType = IntervalTypes.Days
                myXax.Interval = 1

                If blnAddMarkers Then
                    ser.MarkerEnabled = True
                Else
                    ser.MarkerEnabled = False
                End If
                ser.XValueType = ChartValueTypes.DateTime
                ser.ToolTipText = "#YValue, #Series"
                If i = 0 Then
                    ser.AxisYType = AxisTypes.Primary
                Else
                    ser.AxisYType = AxisTypes.Secondary
                End If
            Else
                '  myXax.IntervalType = IntervalTypes.Days
                myXax.Interval = 1
            End If

            ser.MarkerColor = Brushes.White
            ser.MarkerBorderColor = Brushes.Black
            ser.MarkerSize = 10
            If Not IsNothing(sLineColor(i + 1)) Then
                If sLineColor(i + 1).ToString <> Brushes.Transparent.ToString Then
                    ser.Color = sLineColor(i + 1)
                End If
            End If
            ser.LineThickness = 2
            ser.ShadowEnabled = False
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
                    If ser.RenderAs = RenderAs.Column Or ser.RenderAs = RenderAs.Bar Then
                        '  dp.AxisXLabel = sDates(j)
                        dp.AxisXLabel = sDates_as(i, j)
                        dp.ToolTipText = ser.Name & ",#YValue"
                        ser.LabelEnabled = True
                        ser.LabelText = "#YValue"
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
                        cha.AxesX(0).ValueFormatString = sValueFormatString  '"h:mm"
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
    Private Sub getTrend(ByVal sender As TextBox, ByVal e As System.Windows.RoutedEventArgs)
        Dim sEX As String
        Dim sScale As String
        Dim sQue As String
        Dim sVar As String
        Dim sType As String
        Dim sHiHi As String

        If My.Computer.Keyboard.CtrlKeyDown Then

            '   sTagDrag = sender.Tag
            sEX = config_aa.DocumentElement.SelectSingleNode("input[@name='" & sender.Name & "']").Attributes.GetNamedItem("ext").Value

            strXML = "<variables>"
            strXML = strXML & "<input trend='yes' var='" & sender.Tag & "' ext='" & sEX & "'><value/><time/></input>"
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
                strXML = strXML & "<input trend='yes' hihi='" & sHiHi & "' tag='" & sender.Tag & "' type='" & sType & "' var='" & sVar & "' equ='" & sQue & "' ext='" & sEX & "' scale='" & sScale & "'><value/><time/></input>"
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
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub

    Private Sub getChartData(ByVal iGatherType As Integer, ByVal strXML As String, ByRef iops As Integer, ByRef inum As Integer, ByVal pts As OSIsoft.AF.PI.PIPointList)
        Dim sEx(4) As String
        Dim avalues As AFValues
        Dim avalue As AFValue
        Dim ii As Integer
        Dim config_x As New Xml.XmlDocument
        Dim timerange1 As AFTimeRange

        Try
            '   iops = 3 'ii1 - 1
            'iops = 4
            timerange1 = New AFTimeRange(sTime_start, sTime_end, Globalization.CultureInfo.CurrentCulture)
            If iGatherType = 0 Then
                inum = 0
                For i = 0 To iops - 1
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, pi_col_xls_tags(i + 1), sinusoid) Then
                        'avalues = pts(i).InterpolatedValuesByCount(timerange1, 500, "", False)
                        avalues = sinusoid.InterpolatedValuesByCount(timerange1, 500, "", False)
                        ii = 0
                        For Each avalue In avalues
                            If avalue.IsGood Then
                                rValues_as(i, ii) = FormatNumber(avalue.Value, 2)
                            Else
                                rValues_as(i, ii) = -9
                            End If
                            sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                            ii = ii + 1
                        Next
                        iPoints_as(i) = ii - 1
                    End If
                    ' End If
                Next
                For ii = 0 To iPoints_as(1)
                    rValues_as(1, ii) = rValues_as(1, ii) - rValues_as(2, ii)
                Next

            ElseIf iGatherType = 1 Then

                inum = 0
                For i = 0 To iops - 1
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, pi_col_xls_tags(i + 1), sinusoid) Then
                        avalues = sinusoid.RecordedValues(timerange1, Data.AFBoundaryType.Outside, "", False) ' 500, "", False)
                        ii = 0
                        For Each avalue In avalues
                            If avalue.IsGood Then
                                rValues_as(i, ii) = FormatNumber(avalue.Value, 0)
                            Else
                                rValues_as(i, ii) = -9
                            End If
                            sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                            ii = ii + 1
                        Next
                        iPoints_as(i) = ii - 1
                    End If
                Next
            ElseIf iGatherType = 2 Then

                inum = 0
                For i = 0 To iops - 1
                    avalue = pts(i).CurrentValue
                    ii = 0
                    rValues_as(i, ii) = avalue.Value
                    ' sDates_as(i, ii) = avalue.Timestamp.LocalTime.ToString
                    iPoints_as(i) = 1
                Next
            End If
        Catch ex As Exception

        End Try
        ' avalues.Clear()
        '  avalues = Nothing
        ' config_x = Nothing
        ' sinusoid = Nothing
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
        Dim sTag1, sTag2 As String

        Dim rand As Random
        Dim imagePath As String

        dTime = (Now - kida.ElementTime)


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
                        Call UpdateLiveChart("chart_flows_in", 1)
                        Call UpdateLiveChart("chart_flows_lean", 2)
                        ''     Call UpdateLiveChart("chart_flows_acid", 3)
                    Else
                        If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                            myP = sinusoid.CurrentValue()
                            If myP.IsGood Then
                                element = kida.ElementXML.Clone
                                Call elementTest(element, myP.Value, sType, sEx)
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
                    If kida.ElementHiHI <> "none" And IsNumeric(kida.ElementHiHI) Then
                        If kida.ElementXML.SelectSingleNode("PV").InnerText > CSng(kida.ElementHiHI) Then
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "red"
                        Else
                            kida.ElementXML.SelectSingleNode("flag_fg").InnerText = "#FF000042"
                        End If
                    End If
                ElseIf sType = "rec_analog" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        '   Call elementTest(element, myP.Value, sType, sEx)
                        If myP.Value <= 0 Then
                            kida.ElementObject.Height = 49
                        Else
                            kida.ElementObject.Height = (1 - myP.Value / 100) * 49
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
                        Call elementTest(element, myP.Value, sType, sEx)
                        kida.ElementXML = element
                    End If
                ElseIf sType = "calc" Then
                    '  Call Subs(kida.ElementName)
                ElseIf sType = "plm" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(element, myP.Value, sType, sEx)
                        kida.ElementXML = element
                    End If
                ElseIf sType = "string" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(element, myP.Value, sType, sEx)
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
                        Call elementTest(element, myP.Value, "status", sEx)
                        kida.ElementXML = element
                    Else
                        kida.ElementXML.SelectSingleNode("cursta").InnerText = "error"
                    End If
                    '  kida.ElementObject.DataContext = Nothing
                ElseIf sType = "status_analog" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(element, myP.Value, "status_analog", sEx)
                        kida.ElementXML = element
                    End If
                ElseIf sType = "animation" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        element = kida.ElementXML.Clone
                        Call elementTest(element, myP.Value, "animation", sEx)
                        kida.ElementXML = element
                        If element.SelectSingleNode("cursta").InnerText = "on" Then
                            kida.ElementObject.RenderTransform = oTransform1
                            imagePath = "/CDB_Overview;component/Images/wheel_bold_on.png"
                        Else
                            kida.ElementObject.RenderTransform = oTransform0
                            imagePath = "/CDB_Overview;component/Images/wheel.png"
                        End If

                        Dim newbitimg As New BitmapImage
                        Dim newimg As Image = kida.ElementObject

                        newbitimg.BeginInit()
                        newbitimg.UriSource = New Uri(imagePath, UriKind.RelativeOrAbsolute)
                        newbitimg.EndInit()
                        newimg.Source = newbitimg
                    End If
                ElseIf sType = "animation_analog" Then
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(srvAF, kida.ElementPITag, sinusoid) Then
                        myP = sinusoid.CurrentValue()
                        If myP.IsGood Then
                            element = kida.ElementXML.Clone
                            Call elementTest(element, myP.Value, "animation_analog", sEx)
                            kida.ElementXML = element
                            If element.SelectSingleNode("cursta").InnerText = "on" Then
                                ' oWeelAuto1.RepeatBehavior = New RepeatBehavior(100)

                                kida.ElementObject.BeginAnimation(Image.OpacityProperty, oWeelAuto2)
                            Else
                                '        oWeelAuto1.RepeatBehavior = New RepeatBehavior(0)
                                kida.ElementObject.Opacity = 0
                            End If
                        Else
                            kida.ElementObject.RenderTransform = oTransform0
                        End If

                    End If
                End If
            Catch ex As Exception
                ncount = 0
            End Try

        End If
        If Not blnStop Then
            kida.ElementObject.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, New SubPrimeDelegate(AddressOf ThreadStartTimer_kid), kida)
        End If


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

            If atemp.Count > 1 Then
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
    Private Sub elementTest(ByRef element As Xml.XmlNode, ByVal curval As Object, ByVal sType As String, ByVal sEx As String)
        ' Dim element As Xml.XmlNode = aak1.ElementXML.Clone
        'Dim sEx As String
        If curval.GetType.Name = "Single" Or curval.GetType.Name = "Int16" Or curval.GetType.Name = "Int32" Or curval.GetType.Name = "AFEnumerationValue" Then
            If sType = "analog" Or sType = "batmtr" Or sType = "plm" Or sType = "tank" Then
                Try
                    If element.SelectSingleNode(sEx).InnerText <> CStr(curval) Then
                        element.SelectSingleNode(sEx).InnerText = CStr(curval)
                        ' check for limits

                    End If
                Catch ex As Exception

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
                        ElseIf curval.ToString = "Inbet" Then
                            element.SelectSingleNode("cursta").InnerText = "error"
                        ElseIf curval.ToString = "RUN" Or curval.ToString = "ON" Then
                            element.SelectSingleNode("cursta").InnerText = "open"
                        ElseIf curval.ToString = "STOP" Or curval.ToString = "OFF" Then
                            element.SelectSingleNode("cursta").InnerText = "closed"
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
            ElseIf sType = "animation" Then

                If curval.GetType.Name = "AFEnumerationValue" Then
                    If curval.ToString = "RUN" Or curval.ToString = "ON" Then
                        element.SelectSingleNode("cursta").InnerText = "on"
                    ElseIf curval.ToString = "STOP" Or curval.ToString = "OFF" Then
                        element.SelectSingleNode("cursta").InnerText = "off"
                    End If
                End If
            ElseIf sType = "animation_analog" Then

                If CSng(curval) > 0 Then
                    element.SelectSingleNode("cursta").InnerText = "on"
                ElseIf curval.ToString = "STOP" Then
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
    Private Sub UpdateLiveChart(ByRef chaName As String, ByVal ipt As Integer)
        ' get series names

        Dim ser As DataSeries
        Dim i1 As Integer = 0

        Dim dp As DataPoint
        Dim cha As Chart

        Dim results As OSIsoft.AF.AFListResults(Of OSIsoft.AF.PI.PIPoint, OSIsoft.AF.Asset.AFValue)

        cha = LogicalTreeHelper.FindLogicalNode(mycanvas, chaName)

        results = pts(ipt).CurrentValue

        For Each ser In cha.Series

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
        '   cha.AxesY(0).AxisMaximum = rmax * 1.1
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

    Private Sub Scot_stack1_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        oTransform1.BeginAnimation(RotateTransform.AngleProperty, oWeelAuto1)
        oTransform0.BeginAnimation(RotateTransform.AngleProperty, oWeelAuto0)

        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - 20) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale



    End Sub

    Private Sub Scot_stack1_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        xxscale = (sender.actualWidth - 10) / mycanvas.Width
        yyscale = (sender.actualHeight - 20) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale
    End Sub
End Class
