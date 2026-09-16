// Formats raw digits into the backend's required CNIC shape:
// 12345-1234567-1 (matches the RegularExpression on CreateResidentRequest
// and FamilyMemberRequest: ^\d{5}-\d{7}-\d$).
export function formatCnic(rawValue: string): string {
  const digits = rawValue.replace(/\D/g, '').slice(0, 13);

  const part1 = digits.slice(0, 5);
  const part2 = digits.slice(5, 12);
  const part3 = digits.slice(12, 13);

  return [part1, part2, part3].filter(Boolean).join('-');
}
