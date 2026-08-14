using Domain.Enum;

namespace Domain.Aggregates.MatchAggregate;

public sealed class Dismissal
{
    public WicketType WicketType { get; }
    public int? FielderId { get; }

    private Dismissal()
    {
    }

    private Dismissal(WicketType wicketType, int? fielderId)
    {
        Validate(wicketType, fielderId);
        WicketType = wicketType;
        FielderId = fielderId;
    }

    public static Dismissal Create(WicketType wicketType,int? fielderId)
    {
        return new Dismissal(wicketType,fielderId);
    }

    private static void Validate(WicketType wicketType,int? fielderId)
    {
	    if (wicketType == WicketType.None)
        throw new ArgumentException("Dismissal type cannot be None.", nameof(wicketType));
      
      bool requiresFielder = wicketType == WicketType.Caught || wicketType == WicketType.RunOut || wicketType == WicketType.Stumped; 
      
      if (requiresFielder && (!fielderId.HasValue || fielderId <= 0)) { 
	      throw new ArgumentException( "Fielder is required for this dismissal type.", nameof(fielderId)); } 
      if (!requiresFielder && fielderId.HasValue) { 
	      throw new ArgumentException( "Fielder is not applicable for this dismissal type.", nameof(fielderId)); }
    }
}