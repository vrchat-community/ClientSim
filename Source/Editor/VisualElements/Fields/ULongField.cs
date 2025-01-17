using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.Fields
{
  public class ULongField : TextValueField<ulong>
  {
    /// <summary>
    ///   <para>
    ///     USS class name of elements of this type.
    ///   </para>
    /// </summary>
    public new static readonly string ussClassName = "vrc-ulong-field";
    /// <summary>
    ///   <para>
    ///     USS class name of labels in elements of this type.
    ///   </para>
    /// </summary>
    public new static readonly string labelUssClassName = ULongField.ussClassName + "__label";
    /// <summary>
    ///   <para>
    ///     USS class name of input elements in elements of this type.
    ///   </para>
    /// </summary>
    public new static readonly string inputUssClassName = ULongField.ussClassName + "__input";

    private ULongField.ULongInput sbyteInput => (ULongField.ULongInput) this.textInputBase;

    /// <summary>
    ///   <para>
    ///     Converts the given integer to a string.
    ///   </para>
    /// </summary>
    /// <param name="v">The integer to be converted to string.</param>
    /// <returns>
    ///   <para>The integer as string.</para>
    /// </returns>
    protected override string ValueToString(ulong v) => v.ToString(this.formatString, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);

    /// <summary>
    ///   <para>
    ///     Converts a string to an integer.
    ///   </para>
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>
    ///   <para>The integer parsed from the string.</para>
    /// </returns>
    protected override ulong StringToValue(string str)
    {
      ulong num;
      return ClientSimUINumericFieldsUtils.TryConvertStringToULong(str, this.textInputBase.text, out num) ? num : this.rawValue;
    }

    /// <summary>
    ///   <para>
    ///     Constructor.
    ///   </para>
    /// </summary>
    public ULongField()
      : this((string) null)
    {
    }

    /// <summary>
    ///   <para>
    ///     Constructor.
    ///   </para>
    /// </summary>
    /// <param name="maxLength">Maximum number of characters the field can take.</param>
    public ULongField(int maxLength)
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
    public ULongField(string label, int maxLength = -1)
      : base(label, maxLength, (TextValueField<ulong>.TextValueInput) new ULongField.ULongInput())
    {
      this.AddToClassList(UIntField.ussClassName);
      this.labelElement.AddToClassList(UIntField.labelUssClassName);
      this.AddLabelDragger<ulong>();
    }

    /// <summary>
    ///   <para>
    ///     Applies the values of a 3D delta and a speed from an input device.
    ///   </para>
    /// </summary>
    /// <param name="delta">A vector used to compute the value change.</param>
    /// <param name="speed">A multiplier for the value change.</param>
    /// <param name="startValue">The start value.</param>
    public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue) => this.sbyteInput.ApplyInputDeviceDelta(delta, speed, startValue);

    /// <summary>
    ///   <para>
    ///   Instantiates an IntegerField using the data read from a UXML file.
    ///   </para>
    /// </summary>
    public new class UxmlFactory : UnityEngine.UIElements.UxmlFactory<ULongField, ULongField.UxmlTraits>
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

    private class ULongInput : TextValueField<ulong>.TextValueInput
    {
      private ULongField parentULongField => (ULongField) this.parent;

      internal ULongInput() => this.formatString = ClientSimUINumericFieldsUtils.k_ByteFieldFormatString;

      protected override string allowedCharacters => ClientSimUINumericFieldsUtils.k_AllowedCharactersForByte;

      public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
      {
        double intDragSensitivity = (double) ClientSimNumericFieldDraggerUtility.CalculateIntDragSensitivity((long) startValue);
        float acceleration = ClientSimNumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
        long num = (long) this.StringToValue(this.text) + (long) Math.Round((double) ClientSimNumericFieldDraggerUtility.NiceDelta((Vector2) delta, acceleration) * intDragSensitivity);
        if (this.parentULongField.isDelayed)
          this.text = this.ValueToString((uint)Mathf.Clamp(num,uint.MinValue,uint.MaxValue));
        else
          this.parentULongField.value = (uint)Mathf.Clamp(num,uint.MinValue,uint.MaxValue);
      }

      protected override string ValueToString(ulong v) => v.ToString(this.formatString);

      protected override ulong StringToValue(string str)
      {
        ulong num;
        ClientSimUINumericFieldsUtils.TryConvertStringToULong(str, this.text, out num);
        return num;
      }
    }
  }
}
