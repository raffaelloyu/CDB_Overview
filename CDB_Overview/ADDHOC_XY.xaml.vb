Imports System
Imports System.Windows
Imports System.Data
Imports Visifire.Charts
Imports System.Windows.Threading
Imports System.Threading

Imports System.Globalization

Imports OSIsoft.AF
Imports OSIsoft.AF.PI
Imports OSIsoft.AF.Asset
Imports OSIsoft.AF.Search
Imports OSIsoft.AF.Time
Imports OSIsoft.AF.Data

Imports Excel = Microsoft.Office.Interop.Excel
Public Class ADDHOC_XY
    Inherits Window

    Private Delegate Sub SubChartPrimeDelegate(ByRef kida As Chart)

    Private piservers As New PIServers
    Private pisystems As New PISystems
    Private PISystem As PISystem
    Private PIServer As PIServer

    Private cafops As AFDatabase
    Private sinusoid As OSIsoft.AF.PI.PIPoint
    Private timerange As AFTimeRange
    Private values As AFValues

    ' Private mySDK As New PISDK.PISDK
    '  Private srv As Server
    Private srvname As String = "cpi1-t"

    Private srvAF As OSIsoft.AF.PI.PIServer

    Public Delegate Sub NextPrimeDelegate()
    Private sTagName As String
    Private strConnectionString As String
    Private strSQL_a As String
    Private strSQL_t As String
    Private strSQL_v As String


    Private strName As String
    Private config_aa As New Xml.XmlDocument
    Private WithEvents timer1 As New DispatcherTimer

    ' Private chart As New Visifire.Charts.Chart

    Private dataSeries As New DataSeries
    Private iCount As Integer = 0
    Private blnInit As Boolean = True
    Private _value As Single
    Private _time As Date
    Private blnReady As Boolean = False

    'Private ds As New DataSeries
    'Dim dpcol As New DataPointCollection

    Private myXax As New Axis
    Private myYax As New Axis
    Private myYax_r As New Axis

    Private strXML As String = ""
    Private sConsole As String
    Private sTitle As String
    Private sLineColors(20) As Brush
    Private iType As Integer
    Private sfilename As String

    Private sVBS As String
    Private sTime_start As Date
    Private sTime_end As Date

    Private sTags() As String
    Private seQus() As String
    Private sScales() As String
    Private sXYTypes() As String

    Private iTags As Integer = 0
    Private iseries As Integer = -1
    Private pi_col As New Collection
    Private pi_col_xls_tags As String()
    Private pi_col_xls_tags_UOM As String()
    Private pi_col_xls_tags_desc As String()

    Private iShift As Integer
    Private _sServerName As String
    Private foundPoints As IEnumerable(Of OSIsoft.AF.PI.PIPoint)
    Private pts As New OSIsoft.AF.PI.PIPointList
    Private pts1 As New OSIsoft.AF.PI.PIPointList

    Private iops, iPoints As Integer
    Private rValues_as(6, 0) As Double
    Private sDates_as(6, 0) As String
    Private iPoints_as(6) As Integer

    Private sDates(0) As String
    Private blnAddMarkers, blnAddLabels, blnAddChart As Boolean
    Private blnEqu() As Boolean
    Private cha As Chart
    Private xlnIn As New Xml.XmlDocument
    Private _xmlfile As String
    Public Sub New(ByVal _strXML As String, ByVal sServerName As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _sServerName = sServerName
        srvAF = OSIsoft.AF.PI.PIServer.FindPIServer(sServerName)

        Dim stemp As String
        For i = 0 To 23
            If i < 10 Then
                stemp = "0" & i & ":00"
            Else
                stemp = i & ":00"
            End If
            comStartHours.Items.Add(stemp)
            comEndHours.Items.Add(stemp)
            comDeltaHours.Items.Add(i + 1)
        Next
        comDeltaHours.SelectedIndex = 7

        If Now.Hour < 10 Then
            stemp = "0" & Now.Hour
        Else
            stemp = Now.Hour
        End If

        If Now.Minute < 10 Then
            stemp = stemp & ":" & "0" & Now.Minute
        Else
            stemp = stemp & ":" & Now.Minute
        End If

        mycanvas.Width = 1300
        mycanvas.Height = 700


        comStartHours.Text = stemp
        comEndHours.Text = stemp

        iShift = 6

        dpEnd.Text = Now.Date.ToString
        dpStart.Text = DateAdd(DateInterval.Day, -7, Now.Date).Date.ToString

        sTime_start = DateAdd(DateInterval.Hour, -iShift, DateAdd(DateInterval.Minute, 2, Now))
        sTime_end = Now ' DateAdd(DateInterval.Hour, 2, sTimes(ii))
        timerange = New AFTimeRange(sTime_start, sTime_end, CultureInfo.CurrentCulture)

        sLineColors(0) = Brushes.DarkBlue 'Violat
        sLineColors(1) = Brushes.DarkBlue
        sLineColors(2) = Brushes.Green
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

        ReDim rValues_as(8, 5000)
        ReDim sDates_as(8, 5000)
        ReDim iPoints_as(8)

        strXML = _strXML

        '   For i = 1 To 10
        ' sTags(i) = ""
        ' sScales(i) = "1"
        ' Next

        config_aa.LoadXml(strXML)
        stemp = ""
        Dim ii1 As Integer = 0

        For Each nnode In config_aa.SelectNodes("//input")
            stemp = stemp & " " & UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
            ReDim Preserve sTags(ii1)
            ReDim Preserve sScales(ii1)
            ReDim Preserve blnEqu(ii1)
            ReDim Preserve seQus(ii1)
            Try
                sScales(ii1) = nnode.attributes.getnameditem("scale").value.ToString
            Catch ex As Exception
                sScales(ii1) = 1
            End Try
            Try
                sXYTypes(ii1) = nnode.attributes.getnameditem("xytype").value.ToString
            Catch ex As Exception
                sXYTypes(ii1) = "none"
            End Try
            Try
                If nnode.attributes.getnameditem("equ").value <> "" Then
                    blnEqu(ii1) = True
                    sTags(ii1) = nnode.attributes.getnameditem("var").value.ToString
                    seQus(ii1) = nnode.attributes.getnameditem("equ").value.ToString
                    stemp = nnode.attributes.getnameditem("tag").value.ToString
                Else
                    sTags(ii1) = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                    blnEqu(ii1) = False
                End If
            Catch ex As Exception

                sTags(ii1) = UCase(nnode.attributes.getnameditem("var").value.ToString) & "." & UCase(nnode.attributes.getnameditem("ext").value.ToString)
                blnEqu(ii1) = False

            End Try
            ii1 = ii1 + 1
        Next

        Try
            sTitle = config_aa.DocumentElement.Attributes("title").Value
        Catch ex As Exception
            sTitle = stemp
        End Try

        iops = ii1

        If iops = 1 Then
            txtTag.Text = sTitle
        Else
            txtTag.Text = ""
        End If

        myYax.ViewportRangeEnabled = True
        blnInit = True

        '   exportXL.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf sendXL))
        '   recLeft.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf slideLeft))
        '  recRight.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf slideLeft))
        '  imgShowList.AddHandler(PreviewMouseDownEvent, New RoutedEventHandler(AddressOf showList))

        chkEnableZoom.Visibility = Visibility.Hidden


        'lbxPLMEvents.DataContext = xlnIn.DocumentElement.SelectSingleNode("input").Attributes("var")

        blnAddMarkers = True
        If iops > 1 Then
            blnAddMarkers = False
        End If

        blnAddLabels = False
        blnAddChart = True

        Call CreateChart_xy()

        timer1.IsEnabled = True
        timer1.Interval = TimeSpan.FromSeconds(10)
    End Sub
    Private Sub CreateChart_xy()
        Dim times() As AFTime
        Dim inum As Integer = 0
        Dim itemp As Integer = 50
        Dim avalues_x, avalues_y As AFValues

        'create times arrays
        inum = timerange.Span.TotalMinutes / 50
        If inum > 2 Then
            ReDim times(inum)
            For ii = 1 To inum
                times(ii) = DateAdd(DateInterval.Minute, itemp * ii, timerange.StartTime)
            Next
            For ij = 1 To sTags.Count - 1
                If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(PIServer, sTags(ij), sinusoid) Then
                    If sXYTypes(ij) = "xvalues" Then
                        avalues_x = sinusoid.InterpolatedValuesAtTimes(times, "", False)
                    Else
                        avalues_y = sinusoid.InterpolatedValuesAtTimes(times, "", False)
                    End If
                End If
            Next
        End If
    End Sub
End Class
