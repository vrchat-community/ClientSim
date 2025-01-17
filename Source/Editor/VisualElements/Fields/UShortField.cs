using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.Fields
{
  public class UShortField : TextValueField<ushort>
  {
    /// <summary>
    ///   <para>
    ///     USS class name of elements of this type.
    ///   </para>
    /// </summary>
    public new static readonly string ussClassName = "vrc-ushort-field";
    /// <summary>
    ///   <para>
    ///     USS class name of labels in elements of this type.
    ///   </para>
    /// </summary>
    public new static readonly string labelUssClassName = UShortField.ussClassName + "__label";
    /// <summary>
    ///   <para>
    ///     USS class name of input elements in elements of this type.
    ///   </para>
    /// </summary>
    public new static readonly string inputUssClassName = UShortField.ussClassName + "__input";

    private UShortField.UShortInput ushortInput => (UShortField.UShortInput) this.textInputBase;

    /// <summary>
    ///   <para>
    ///     Converts the given integer to a string.
    ///   </para>
    /// </summary>
    /// <param name="v">The integer to be converted to string.</param>
    /// <returns>
    ///   <para>The integer as string.</para>
    /// </returns>
    protected override string ValueToString(ushort v) => v.ToString(this.formatString, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);

    /// <summary>
    ///   <para>
    ///     Converts a string to an integer.
    ///   </para>
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>
    ///   <para>The integer parsed from the string.</para>
    /// </returns>
    protected override ushort StringToValue(string str)
    {
      ushort num;
      return ClientSimUINumericFieldsUtils.TryConvertStringToUShort(str, this.textInputBase.text, out num) ? num : this.rawValue;
    }

    /// <summary>
    ///   <para>
    ///     Constructor.
    ///   </para>
    /// </summary>
    public UShortField()
      : this((string) null)
    {
    }

    /// <summary>
    ///   <para>
    ///     Constructor.
    ///   </para>
    /// </summary>
    /// <param name="maxLength">Maximum number of characters the field can take.</param>
    public UShortField(int maxLength)
      : this((string) null, maxLength)
    {
    }

    /// <summary>
    ///   <para>
    ///     Constructor.
    ///   </para>
    /// </summary>
    /// <param name="maxLength">Maximum number of characters the field can take.</param>
    /// <param name="label"></param>
    public UShortField(string label, int maxLength = -1)
      : base(label, maxLength, (TextValueField<ushort>.TextValueInput) new UShortField.UShortInput())
    {
      this.AddToClassList(UShortField.ussClassName);
      this.labelElement.AddToClassList(UShortField.labelUssClassName);
      this.AddLabelDragger<ushort>();
    }

    /// <summary>
    ///   <para>
    ///     Applies the values of a 3D delta and a speed from an input device.
    ///   </para>
    /// </summary>
    /// <param name="delta">A vector used to compute the value change.</param>
    /// <param name="speed">A multiplier for the value change.</param>
    /// <param name="startValue">The start value.</param>
    public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ushort startValue) => this.ushortInput.ApplyInputDeviceDelta(delta, speed, startValue);

    /// <summary>
    ///   <para>
    ///     Instantiates an IntegerField using the data read from a UXML file.
    ///   </para>
    /// </summary>
    public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<UShortField, UShortField.UxmlTraits>
    {
    }

    /// <summary>
    ///   <para>
    ///     Defines UxmlTraits for the IntegerField.
    ///   </para>
    /// </summary>
    public new class UxmlTraits : TextValueFieldTraits<int, UxmlIntAttributeDescription>
    {
    }

    private class UShortInput : TextValueField<ushort>.TextValueInput
    {
      private UShortField parentByteField => (UShortField) this.parent;

      internal UShortInput() => this.formatString = ClientSimUINumericFieldsUtils.k_ByteFieldFormatString;

      protected override string allowedCharacters => ClientSimUINumericFieldsUtils.k_AllowedCharactersForByte;

      public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ushort startValue)
      {
        double intDragSensitivity = (double) ClientSimNumericFieldDraggerUtility.CalculateIntDragSensitivity((long) startValue);
        float acceleration = ClientSimNumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
        long num = (long) this.StringToValue(this.text) + (long) Math.Round((double) ClientSimNumericFieldDraggerUtility.NiceDelta((Vector2) delta, acceleration) * intDragSensitivity);
        if (this.parentByteField.isDelayed)
          this.text = this.ValueToString((ushort)Mathf.Clamp(num,0,255));
        else
          this.parentByteField.value = (ushort)Mathf.Clamp(num,0,255);
      }

      protected override string ValueToString(ushort v) => v.ToString(this.formatString);

      protected override ushort StringToValue(string str)
      {
        ushort num;
        ClientSimUINumericFieldsUtils.TryConvertStringToUShort(str, this.text, out num);
        return num;
      }
    }
  }
}
