Public Class BGNumberConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If parameter IsNot Nothing Then
            Dim strCompare As String = parameter.ToString
            Dim ssign As String = Mid(strCompare, 1, 2)
            Dim rCompare As Single = CSng(Mid(strCompare, 3))
            If IsNumeric(value) Then
                If Not String.IsNullOrEmpty(rCompare) Then
                    ' Return String.Format(culture, strFormatString, CSng(value))
                    If value.GetType.Name = "Boolean" Then
                        If value Then
                            Return 1
                        Else
                            Return -1
                        End If
                    Else
                        If ssign = "lt" Then
                            If CSng(value) < rCompare Then
                                Return -1
                            Else
                                Return 1
                            End If
                        ElseIf ssign = "gt" Then
                            If CSng(value) > rCompare Then
                                Return -1
                            Else
                                Return 1
                            End If
                        ElseIf ssign = "eq" Then
                            If CSng(value) = rCompare Then
                                Return -1
                            Else
                                Return 1
                            End If
                        End If
                    End If
                End If
            ElseIf value.GetType.Name = "Boolean" Then
                Return 0
            ElseIf value.GetType.Name = "String" Then
                Return -1
            End If


        End If

        Return value.ToString

    End Function

    ''' <summary>
    ''' Attempts to convert the value back using a type specific TypeConverter
    ''' </summary>
    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Dim objTypeConverter As System.ComponentModel.TypeConverter = System.ComponentModel.TypeDescriptor.GetConverter(targetType)
        Dim objReturnValue As Object = Nothing

        If objTypeConverter.CanConvertFrom(value.[GetType]()) Then
            objReturnValue = objTypeConverter.ConvertFrom(value)
        End If

        Return objReturnValue

    End Function
End Class
