export function getInitials(fullName: string): string {
  const parts = fullName.trim().split(/\s+/); // split on any whitespace, handles extra spaces

  if (parts.length === 1) {
    // Single-word name ("Madonna") — just take the first letter
    return parts[0].charAt(0).toUpperCase();
  }

  // Multi-word name ("Sushit Chaulagain") — first letter of first word +
  // first letter of LAST word (handles middle names correctly, e.g.
  // "John Michael Smith" -> "JS", not "JM")
  const first = parts[0].charAt(0);
  const last = parts[parts.length - 1].charAt(0);
  return (first + last).toUpperCase();
}
