Imports System.Globalization

Public Class SalesGasConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Return Format(value * 100, "#.##")

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Dim objTypeConverter As System.ComponentModel.TypeConverter = System.ComponentModel.TypeDescriptor.GetConverter(targetType)
        Dim objReturnValue As Object = Nothing

        If objTypeConverter.CanConvertFrom(value.[GetType]()) Then
            objReturnValue = objTypeConverter.ConvertFrom(value)
        End If

        Return objReturnValue
    End Function
End Class
