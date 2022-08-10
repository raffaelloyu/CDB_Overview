Imports OSIsoft.AF
Imports OSIsoft.AF.PI
Imports OSIsoft.AF.Asset
Imports OSIsoft.AF.Search
Imports OSIsoft.AF.Time
Imports OSIsoft.AF.Data
Imports System.Windows.Threading
Imports System.Threading

Imports System.IO
Imports System.Globalization
Imports System.ComponentModel

Imports Microsoft.Office.Interop
Public Class DataExport
    Inherits Window
    Private piservers As New PIServers
    Private pisystems As New PISystems
    Private PISystem As PISystem
    Private PIServer As PIServer
    Private cafops As AFDatabase
    Private alarms As AFNamedCollectionList(Of AFElement)
    Private rtus As AFNamedCollectionList(Of AFElement)
    Private statuses As AFNamedCollectionList(Of AFElement)
    Private groups As AFNamedCollectionList(Of AFElement)
    Private cmxs As AFNamedCollectionList(Of AFElement)
    Private newtree As TreeViewItem
    Private newtree1 As TreeViewItem
    Private newtree2 As TreeViewItem
    Private blnClear = True

    Private sName, sTag As String
    Private Pi_values_xls As AFValues
    Private sinusoid As OSIsoft.AF.PI.PIPoint
    Private timerange As AFTimeRange
    Private avalues As AFValues

    Private basenode As AFElement

    Private blnStart As Boolean = True
    Private sNames(), sAnalogNames(), sTags() As String
    Private iTagAnalogs As Integer = 0
    Private iTags As Integer = 4

    Private _PI_min As Double
    Private _PI_max As Double
    Private _PI_mean As Double
    Private _PI_std As Double
    Private _PI_filter As Double
    Private pi_col_xls As New Collection
    Private pi_col_xls_tags() As String
    Private blnfirst As Boolean = True

    Private sTime_start As Date
    Private sTime_end As Date

    Private returnValue As IDictionary(Of AFSummaryTypes, AFValue)
    Private sUser As String
    Private mygrid As Microsoft.Windows.Controls.DataGrid
    Private strXML As String

    Private sServerName As String

    Public Sub New(ByVal _sUser As String)



        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        mycanvas.Width = 1632
        mycanvas.Height = 853.5
        'Height="853.5" Width="1632" 

        sUser = _sUser

        comInterType.Items.Add("Raw Data")
        comInterType.Items.Add("Number of Points")
        comInterType.Items.Add("Time Interval (min)")
        comInterType.Text = "Number of Points"

        sServerName = "pnwpappv003"
        PIServer = piservers(sServerName)

        PISystem = pisystems("CDBNADW8GAP07") '.DefaultPISystem
        PISystem.Connect()
        cafops = PISystem.Databases("CDB")

        dpEnd.Text = Now.Date.ToString
        dpStart.Text = DateAdd(DateInterval.Month, -1, Now.Date)

        Dim stemp As String
        For i = 0 To 23
            If i < 10 Then
                stemp = "0" & i & ":00"
            Else
                stemp = i & ":00"
            End If
            comStartHours.Items.Add(stemp)
            comEndHours.Items.Add(stemp)
        Next

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


        comStartHours.Text = stemp
        comEndHours.Text = stemp

        txtTagToFind.Items.Add("XHN_*")
        txtTagToFind.Items.Add("XHN_GAS-PLNT*")
        txtTagToFind.Items.Add("XHN_SLFR-PLNT*")
        txtTagToFind.Items.Add("XHN_WP-A*")
        txtTagToFind.Items.Add("XHN_WP-C*")
        txtTagToFind.Items.Add("XHN_WP-F*")

        If blnStart Then
            blnStart = False

            Call getTree()
        End If
    End Sub

    Private Sub getTree()
        Dim dummynode As Object = Nothing
        'groups = AFElement.FindElements(cafops, cafops.Elements("GROUPRTU"), "*", Nothing, cafops.ElementTemplates("group"), True, AFSortField.Name, AFSortOrder.Ascending, 40)
        '  groups = AFElement.FindElements(cafops, cafops.Elements("Xuanhan 宣汉"), "*", Nothing, Nothing, AFElementType.Any, True, AFSortField.Name, AFSortOrder.Ascending, 100)
        ' cafops.Elements("Xuanhan 宣汉").Elements
        basenode = cafops.Elements("CDB") 'Elements("Xuanhan 宣汉")
        For Each group1 In cafops.Elements("CDB").Elements
            newtree = New TreeViewItem
            newtree.Header = group1.Name
            newtree.Tag = group1.ID
            newtree.FontWeight = FontWeights.Bold

            newtree.AddHandler(TreeViewItem.ExpandedEvent, New RoutedEventHandler(AddressOf openGroup))
            newtree.Items.Add(dummynode)
            Status.Items(0).Items.Add(newtree)
        Next
        Status.Items(0).IsExpanded = False


    End Sub
    Private Sub openGroup(sender As TreeViewItem, e As RoutedEventArgs)
        Dim sName As String
        If blnClear Then
            If sender.IsExpanded Then
                sender.Items.Clear()
            End If
            sName = sender.Header

            basenode = AFElement.FindElement(PISystem, sender.Tag)
            rtus = AFElement.FindElements(cafops, basenode, "*", Nothing, Nothing, AFElementType.Any, False, AFSortField.Name, AFSortOrder.Ascending, 200)
            Dim dummynode As Object = Nothing
            For Each rtu In rtus
                newtree1 = New TreeViewItem
                newtree1.Header = rtu.Name
                ''   newtree1.Name = rtu.Name
                newtree1.Tag = rtu.ID
                newtree1.AddHandler(TreeViewItem.ExpandedEvent, New RoutedEventHandler(AddressOf openNext))
                newtree1.Items.Add(dummynode)

                sender.Items.Add(newtree1)
            Next
            ' basenode = basenode.Elements(sName)
            sender.IsExpanded = True

        End If
        blnClear = True
        '  lblSelectedStatus.Content = ""

    End Sub
    Private Sub openNext(sender As TreeViewItem, e As RoutedEventArgs)
        Dim sName As String
        Dim sAtt As AFAttribute
        Dim sEx As String

        If blnClear Then
            If sender.IsExpanded Then
                sender.Items.Clear()
            End If
            sName = sender.Header
            basenode = AFElement.FindElement(PISystem, sender.Tag)

            If basenode.Attributes.Count > 0 Then

                For Each sAtt In basenode.Attributes
                    If Not IsNothing(sAtt.DataReference) Then
                        If sAtt.DataReference.Name = "PI Point" Then
                            sEx = sAtt.Name
                            newtree2 = New TreeViewItem
                            ' newtree2.Header = sAtt.Name & "." & sEx & "   " & stat.Attributes("description").Element.Description
                            Try
                                newtree2.Header = sAtt.Description
                                newtree2.Tag = sAtt.DataReference.PIPoint.Name
                                sender.Items.Add(newtree2)
                            Catch ex As Exception

                            End Try

                        End If
                    End If
                Next
            End If

            rtus = AFElement.FindElements(cafops, basenode, "*", Nothing, Nothing, AFElementType.Any, False, AFSortField.Name, AFSortOrder.Ascending, 200)
            Dim dummynode As Object = Nothing
                For Each rtu In rtus
                    newtree2 = New TreeViewItem
                    newtree2.Header = rtu.Name
                    ''   newtree1.Name = rtu.Name
                    newtree2.Tag = rtu.ID
                    newtree2.AddHandler(TreeViewItem.ExpandedEvent, New RoutedEventHandler(AddressOf openNext))
                    newtree2.Items.Add(dummynode)

                    sender.Items.Add(newtree2)
                Next
                '   basenode = basenode.Elements(sName)
                sender.IsExpanded = True



        End If
        blnClear = False
        '  lblSelectedStatus.Content = ""

    End Sub



    Private Sub Status_PreviewMouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Status.PreviewMouseDoubleClick

        Try
            If Not sender.selecteditem.hasitems Then
                '  atemp = Split(Status.SelectedValue.header, " ")
                '   sTag = atemp(0)
                sTag = Status.SelectedValue.Tag
                lbxPLMAnalogs.Items.Add(sTag)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnExport_Click(sender As Object, e As RoutedEventArgs) Handles btnExport.Click
        Dim xlApp As Excel.Application = New Excel.Application
        'Dim xlWorkBook As Excel.Workbook
        Dim itemp_f, itemp_t, itemp_max As Integer
        Dim xlWorkBook_temp As Excel.Workbook
        Dim itemp, inum As Integer

        Dim irow, icol As Integer

        Dim xlWorkSheet, xlWorkSheet2 As Excel.Worksheet
        Dim xlChart As Excel.Chart

        Dim nrange() As Integer

        Dim pi_values_xls As AFValues

        Dim win As System.Security.Principal.WindowsIdentity
        win = System.Security.Principal.WindowsIdentity.GetCurrent()
        'Dim _UserName = win.Name.Substring(win.Name.IndexOf("\") + 1)
        Dim npoints As Integer
        Dim times() As AFTime
        Dim iHour, iminutes As Integer
        Dim pi_tags() As String
        Dim stemp As String = ""
        Dim series1 As Excel.Series
        Dim startDate As New DateTime(1970, 1, 1)
        Dim retnum As Integer

        sTime_start = dpStart.Text
        iHour = CDate(comStartHours.Text).Hour
        sTime_start = DateAdd(DateInterval.Hour, iHour, sTime_start)
        iminutes = CDate(comStartHours.Text).Minute
        sTime_start = DateAdd(DateInterval.Minute, iminutes, sTime_start)

        sTime_end = dpEnd.Text 'Now ' DateAdd(DateInterval.Hour, 2, sTimes(ii))
        iHour = CDate(comEndHours.Text).Hour
        sTime_end = DateAdd(DateInterval.Hour, iHour, sTime_end)
        iminutes = CDate(comEndHours.Text).Minute
        sTime_end = DateAdd(DateInterval.Minute, iminutes, sTime_end)

        timerange = New AFTimeRange(sTime_start, sTime_end, CultureInfo.CurrentCulture)

        ReDim Preserve pi_col_xls_tags(0)
        pi_col_xls.Clear()

        xlWorkBook_temp = xlApp.Workbooks.Add()
        xlWorkSheet = xlWorkBook_temp.Worksheets("Sheet1")
        xlWorkSheet.Name = "PI Data"
        xlWorkSheet2 = xlWorkBook_temp.Worksheets.Add()

        If chkChart.IsChecked Then
            xlChart = xlWorkBook_temp.Charts.Add()
            xlChart.Location(Excel.XlChartLocation.xlLocationAutomatic, Type.Missing)
            xlChart.ChartType = Excel.XlChartType.xlXYScatterLinesNoMarkers
            xlChart.ChartStyle = 2

            With xlChart
                .HasTitle = False
                .HasLegend = True
                .Legend.Position = Excel.XlLegendPosition.xlLegendPositionTop
                .Axes(Excel.XlAxisType.xlValue).HasMajorGridlines = True
                .Axes(Excel.XlAxisType.xlValue).MajorGridlines.Border.LineStyle = Excel.XlLineStyle.xlDash
            End With
        End If

        With xlWorkSheet
            icol = 1
            irow = 1
            .Cells(irow, icol) = "Created"
            .Cells(irow, icol + 1) = Now()

            irow = 2
            .Cells(irow, icol) = "Created by"
            .Cells(irow, icol + 1) = sUser
        End With
        iTags = 0

        iTagAnalogs = 0
        For Each itt In lbxPLMAnalogs.SelectedItems
            iTagAnalogs = iTagAnalogs + 1
            ReDim Preserve pi_tags(iTagAnalogs)
            pi_tags(iTags + iTagAnalogs) = itt
        Next

        If Not IsNothing(pi_tags) Then
            iTagAnalogs = 0
            If comnPoints.Text = "All" Then
                For ij = 1 To pi_tags.Count - 1
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(PIServer, pi_tags(ij), sinusoid) Then
                        avalues = sinusoid.RecordedValues(timerange, AFBoundaryType.Outside, "", False)
                        pi_col_xls.Add(avalues)
                        iTagAnalogs = iTagAnalogs + 1
                        ReDim Preserve pi_col_xls_tags(iTagAnalogs)
                        pi_col_xls_tags(iTagAnalogs) = pi_tags(ij)
                        ReDim Preserve nrange(iTagAnalogs)
                        nrange(iTagAnalogs) = avalues.Count - 1

                    End If
                Next
            ElseIf CInt(comnPoints.Text) > 60 Then
                pi_col_xls.Clear()
                Try
                    npoints = CInt(comnPoints.Text)
                Catch ex As Exception
                    npoints = 200
                End Try

                For ij = 1 To pi_tags.Count - 1
                    If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(PIServer, pi_tags(ij), sinusoid) Then
                        avalues = sinusoid.InterpolatedValuesByCount(timerange, npoints, "", False)
                        pi_col_xls.Add(avalues)
                        iTagAnalogs = iTagAnalogs + 1
                        ReDim Preserve pi_col_xls_tags(iTagAnalogs)
                        pi_col_xls_tags(iTagAnalogs) = pi_tags(ij)

                        ReDim Preserve nrange(iTagAnalogs)
                        nrange(iTagAnalogs) = avalues.Count - 1
                    End If
                Next
            Else
                'create times arrays
                itemp = CInt(comnPoints.Text)
                inum = timerange.Span.TotalMinutes / itemp - 1
                If inum > 2 Then
                    ReDim times(inum)
                    For ii = 1 To inum
                        times(ii) = DateAdd(DateInterval.Minute, itemp * ii, timerange.StartTime)
                    Next
                    For ij = 1 To pi_tags.Count - 1
                        If OSIsoft.AF.PI.PIPoint.TryFindPIPoint(PIServer, pi_tags(ij), sinusoid) Then
                            avalues = sinusoid.InterpolatedValuesAtTimes(times, "", False)
                            pi_col_xls.Add(avalues)
                            iTagAnalogs = iTagAnalogs + 1
                            ReDim Preserve pi_col_xls_tags(iTagAnalogs)
                            pi_col_xls_tags(iTagAnalogs) = pi_tags(ij)

                            ReDim Preserve nrange(iTagAnalogs)
                            nrange(iTagAnalogs) = avalues.Count - 1
                        End If
                    Next
                End If

            End If


            itemp_f = 0
            itemp_t = 0
            irow = 6
            icol = 1

            itemp_t = iTagAnalogs + iTags
            itemp_f = iTags + 1
            For i = itemp_f To itemp_t
                xlWorkSheet.Cells(irow, icol + 1) = pi_col_xls_tags(i)
                icol = icol + 2
                If itemp_max < pi_col_xls(i).count Then
                    itemp_max = pi_col_xls(i).count
                End If
            Next

            icol = 1
            Dim ii1 As Integer = 0
            For i = itemp_f To itemp_t ' Pi_values_xls In pi_col_xls
                pi_values_xls = pi_col_xls(i)
                ii1 = ii1 + 1
                irow = 7

                If chkChart.IsChecked Then
                    retnum = vbYes

                    If nrange(i) > 10000 Then
                        retnum = MsgBox("Tag " & pi_col_xls_tags(i) & " Has more than 10000 data points. Do you want to draw a chart?", MsgBoxStyle.YesNo, "Too many point to trend")
                    End If

                    If retnum = vbYes Then
                        series1 = xlChart.SeriesCollection.NewSeries()
                        stemp = ChrW(64 + (2 * i - 1)) & irow & ":" & ChrW(64 + (2 * i - 1)) & nrange(i)
                        series1.XValues = xlWorkSheet.Range(stemp)
                        series1.Name = pi_col_xls_tags(i)

                        stemp = ChrW(64 + 2 * i) & irow & ":" & ChrW(64 + 2 * i) & nrange(i)
                        series1.Values = xlWorkSheet.Range(stemp)
                    End If
                End If

                Dim tempArray_v As Object(,) = New Object(itemp_max, 2) {}

                For j = 1 To pi_values_xls.Count - 1
                    Try
                        tempArray_v(j, 0) = pi_values_xls.Item(j).Timestamp.LocalTime.ToString
                        If pi_values_xls.Item(j).Value.GetType.Name = "AFEnumerationValue" Then
                            tempArray_v(j, 1) = pi_values_xls.Item(j).Value.value
                        Else
                            tempArray_v(j, 1) = pi_values_xls.Item(j).Value
                        End If
                    Catch ex As Exception

                    End Try
                Next
                Dim myrang As Excel.Range
                myrang = xlWorkSheet.Range(xlWorkSheet.Cells(irow, icol), xlWorkSheet.Cells(irow + itemp_max, icol + 1))
                myrang.Value2 = tempArray_v
                icol = icol + 2
            Next
        End If
        If chkChart.IsChecked Then
            Dim xAxis As Excel.Axis = xlChart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary)
            xAxis.CategoryType = Excel.XlCategoryType.xlTimeScale
            xAxis.HasMajorGridlines = True
            xAxis.TickLabels.NumberFormat = "m/d"

            xAxis.MajorGridlines.Border.LineStyle = Excel.XlLineStyle.xlDash

            xAxis.MinimumScale = timerange.StartTime.UtcTime.ToOADate
            xAxis.MaximumScale = timerange.EndTime.UtcTime.ToOADate

        End If
        xlWorkSheet.Columns.AutoFit()
        xlApp.Visible = True
        ' xlWorkBook_temp.Activate()
        ' xlApp.Quit()
    End Sub

    Private Sub btnSearchTags_Click(sender As Object, e As RoutedEventArgs) Handles btnSearchTags.Click
        sTag = txtTagToFind.Text
        Dim ii1 As Integer = 0

        Dim returnValue As IEnumerable(Of PIPoint)

        returnValue = PIPoint.FindPIPoints(PIServer, sTag)
        dgLPOReasons.DataContext = returnValue

    End Sub
    Private Sub moveTag(sender As Object, e As RoutedEventArgs)
        Try
            Dim sUWI As String = sender.SelectedItem.Name
            lbxPLMAnalogs.Items.Add(sUWI)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub btnSerachType_Click(sender As Object, e As RoutedEventArgs) Handles btnSerachType.Click
        If btnSerachType.Content = "Search by PI Tag" Then
            txtTagToFind.Visibility = Visibility.Visible
            dgLPOReasons.Visibility = Visibility.Visible
            btnSearchTags.Visibility = Visibility.Visible

            btnSerachType.Content = "Search by PI AF"
        Else
            txtTagToFind.Visibility = Visibility.Hidden
            dgLPOReasons.Visibility = Visibility.Hidden
            btnSearchTags.Visibility = Visibility.Hidden

            btnSerachType.Content = "Search by PI Tag"
        End If
    End Sub

    Private Sub btnAddHOC_Click(sender As Object, e As RoutedEventArgs) Handles btnAddHOC.Click
        '  Dim atemp() As String
        Dim itemp As Integer

        strXML = "<variables>"
        For Each itt In lbxPLMAnalogs.SelectedItems
            itemp = InStrRev(itt, ".",)

            ' atemp = Split(itt, ".")
            strXML = strXML & "<input trend='yes' var='" & Mid(itt, 1, itemp - 1) & "' ext='" & Mid(itt, itemp + 1) & "'><value/><time/></input>"
        Next
        strXML = strXML & "</variables>"

        Dim newWinThread As New Thread(AddressOf startADDHOC)
        'strXML = sender.Tag
        newWinThread.IsBackground = True
        newWinThread.SetApartmentState(ApartmentState.STA)
        newWinThread.Start()
    End Sub

    Public Sub startADDHOC()
        Dim newdia As New ADDHOC(strXML, sServerName)
        newdia.Show()
        System.Windows.Threading.Dispatcher.Run()
    End Sub

    Private Sub btnDismiss_Click(sender As Object, e As RoutedEventArgs) Handles btnDismiss.Click

        Me.Close()

    End Sub

    Private Sub bnnClearList_Click(sender As Object, e As RoutedEventArgs) Handles bnnClearList.Click
        lbxPLMAnalogs.Items.Clear()
    End Sub

    Private Sub DataExport_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        '  mygrid = LogicalTreeHelper.FindLogicalNode(mycanvas, "dgLPOReasons")
        '  mygrid.AddHandler(DataGrid.SelectionChangedEvent, New RoutedEventHandler(AddressOf moveTag))

        xxscale = (sender.actualWidth) / mycanvas.Width
        yyscale = (sender.actualHeight) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale
    End Sub

    Private Sub DataExport_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim myscale As ScaleTransform
        Dim xxscale, yyscale As Single

        xxscale = (sender.actualWidth) / mycanvas.Width
        yyscale = (sender.actualHeight) / mycanvas.Height ' SystemParameters.PrimaryScreenHeight '966 '

        myscale = New ScaleTransform(xxscale, yyscale, 0, 0)
        mycanvas.RenderTransform = myscale
    End Sub

    Private Sub comInterType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles comInterType.SelectionChanged
        Dim stemp As String = comInterType.SelectedValue

        comnPoints.Items.Clear()
        If stemp = "Raw Data" Then
            comnPoints.Items.Add("All")
            comnPoints.Text = "All"
        ElseIf stemp = "Number of Points" Then
            comnPoints.Items.Add("200")
            comnPoints.Items.Add("500")
            comnPoints.Items.Add("1000")
            comnPoints.Items.Add("3000")
            comnPoints.Text = 200
        Else
            comnPoints.Items.Add("1")
            comnPoints.Items.Add("5")
            comnPoints.Items.Add("10")
            comnPoints.Items.Add("20")
            comnPoints.Items.Add("60")
            comnPoints.Items.Add("120")
            comnPoints.Text = 10
        End If

    End Sub

    Private Sub dgLPOReasons_PreviewMouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles dgLPOReasons.PreviewMouseDoubleClick
        Try
            Dim sUWI As String = sender.SelectedItem.Name
            lbxPLMAnalogs.Items.Add(sUWI)
        Catch ex As Exception

        End Try
    End Sub



    Private Sub txtTagToFind_KeyDown(sender As Object, e As KeyEventArgs) Handles txtTagToFind.KeyDown
        Dim ii1 As Integer = 0
        Dim returnValue As IEnumerable(Of PIPoint)

        If e.Key = Key.Enter Then 'My.Computer.Keyboard.CtrlKeyDown
            sTag = txtTagToFind.Text
            returnValue = PIPoint.FindPIPoints(PIServer, sTag)
            dgLPOReasons.DataContext = returnValue
        End If
    End Sub


End Class
