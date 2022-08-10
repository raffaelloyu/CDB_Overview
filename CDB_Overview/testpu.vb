Imports System.ComponentModel
Public Class testpu
    Implements INotifyPropertyChanged

    ' These fields hold the values for the public properties.
    Private idValue As Guid = Guid.NewGuid()
    Private testpuName As String = String.Empty
    Private testpuImageSrc As String = String.Empty
    Private testpuValue As String = String.Empty
    Private testpuTag As String = String.Empty
    Private testpuPITag As String = String.Empty
    Private testpuXML As Xml.XmlNode
    Private testpuObject As FrameworkElement
    Private testpuType As String
    Private testpuQ As String
    Private testpuScale As String
    Private testpuEqu As String
    Private testpuHiHi As String
    Private testpuLoLo As String
    Private svalue_new, svalue_old As String
    Private testpuTime As Date
    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
        ' newtxt = LogicalTreeHelper.FindLogicalNode(mycanvas, Me.ElementName.ToString)

    End Sub

    ' The constructor is private to enforce the factory pattern.
    Private Sub New()
        testpuName = "TextBox"
        testpuImageSrc = "none"
        testpuValue = "none"
        testpuTag = "none"
        testpuPITag = "none"
        testpuXML = Nothing
        testpuObject = Nothing
        testpuType = "analog"
        testpuQ = ""
        testpuScale = "1.0"
        testpuEqu = ""
        testpuTime = DateAdd(DateInterval.Day, -1, Now)
    End Sub

    ' This is the public factory method.
    Public Shared Function CreateNewElement() As testpu
        Return New testpu()

    End Function
    Public Property ElementXML() As Xml.XmlNode

        Get
            Return Me.testpuXML
        End Get

        Set(ByVal value As Xml.XmlNode)
            If testpuXML Is Nothing Then
                Me.testpuXML = value
                ' NotifyPropertyChanged("ElementXML")
            Else
                ''If Not (value.OuterXml.ToString = testpuXML.OuterXml.ToString) Then
                'svalue_new = value.SelectSingleNode("curval").InnerText
                'svalue_old = testpuXML.SelectSingleNode("curval").InnerText
                '  If Not (svalue_new = svalue_old) Then
                Me.testpuXML = value
                ' Me.ElementObject.DataContext = Nothing
                NotifyPropertyChanged("ElementXML")
                ''End If
            End If
        End Set
    End Property
    Public Property ElementTime() As Date
        Get
            Return Me.testpuTime
        End Get

        Set(ByVal value As Date)
            '   If Not (value = testpuName) Then
            Me.testpuTime = value
            'NotifyPropertyChanged("ElementName")
            ' End If
        End Set
    End Property

    Public Property ElementName() As String
        Get
            Return Me.testpuName
        End Get

        Set(ByVal value As String)
            '   If Not (value = testpuName) Then
            Me.testpuName = value
            'NotifyPropertyChanged("ElementName")
            ' End If
        End Set
    End Property
    Public Property ElementTag() As String
        Get
            Return Me.testpuTag
        End Get

        Set(ByVal value As String)
            ' If Not (value = testpuTag) Then
            Me.testpuTag = value
            '    NotifyPropertyChanged("ElementTag")
            ' End If
        End Set
    End Property
    Public Property ElementPITag() As String
        Get
            Return Me.testpuPITag
        End Get

        Set(ByVal value As String)
            ' If Not (value = testpuTag) Then
            Me.testpuPITag = value
            '    NotifyPropertyChanged("ElementTag")
            ' End If
        End Set
    End Property

    Public Property ElementValue() As String
        Get
            Return Me.testpuValue
        End Get

        Set(ByVal value As String)
            '  If Not (value = testpuValue) Then
            Me.testpuValue = value
            'NotifyPropertyChanged("ElementValue")
            '  End If
        End Set
    End Property
    Public Property ElementScale() As String
        Get
            Return Me.testpuScale
        End Get

        Set(ByVal value As String)
            '  If Not (value = testpuValue) Then
            Me.testpuScale = value
            'NotifyPropertyChanged("ElementValue")
            '  End If
        End Set
    End Property
    Public Property ElementHiHI() As String
        Get
            Return Me.testpuHiHi
        End Get

        Set(ByVal value As String)
            '  If Not (value = testpuValue) Then
            Me.testpuHiHi = value
            'NotifyPropertyChanged("ElementValue")
            '  End If
        End Set
    End Property
    Public Property ElementLoLo() As String
        Get
            Return Me.testpuLoLo
        End Get

        Set(ByVal value As String)
            '  If Not (value = testpuValue) Then
            Me.testpuLoLo = value
            'NotifyPropertyChanged("ElementValue")
            '  End If
        End Set
    End Property
    Public Property ElementEqu() As String
        Get
            Return Me.testpuEqu
        End Get

        Set(ByVal value As String)
            '  If Not (value = testpuValue) Then
            Me.testpuEqu = value
            'NotifyPropertyChanged("ElementValue")
            '  End If
        End Set
    End Property

    Public Property ElementObject() As FrameworkElement
        Get
            Return Me.testpuObject
        End Get

        Set(ByVal value As FrameworkElement)
            Me.testpuObject = value

        End Set
    End Property

    Public Property ElementType() As String

        Get
            Return Me.testpuType
        End Get

        Set(ByVal value As String)
            Me.testpuType = value

        End Set
    End Property

    Public Property ElementQ() As String

        Get
            Return Me.testpuQ
        End Get

        Set(ByVal value As String)
            Me.testpuQ = value

        End Set
    End Property




End Class