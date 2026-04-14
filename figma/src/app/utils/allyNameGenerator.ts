/**
 * 동료 이름 생성기
 * 기획서: 04_캐릭터_시트_프로토타입.md 참조
 */

export type Gender = 'male' | 'female';
export type JobClass = 'caster' | 'offenser' | 'defender' | 'attacker' | 'priest' | 'shaman';

// 판타지 세계관 이름 풀 (v1)
const maleFirstNames = [
  'Aldric', 'Caelan', 'Dorian', 'Faeron', 'Gareth',
  'Hadric', 'Lorenz', 'Maevik', 'Soren', 'Theron'
];

const maleLastNames = [
  'Ashveil', 'Blackmoor', 'Coldwyn', 'Duskmore', 'Embervane',
  'Greyfall', 'Hollowmere', 'Ironfeld', 'Nighthollow', 'Stormveil'
];

const femaleFirstNames = [
  'Aelith', 'Brynn', 'Ceira', 'Elowen', 'Fayla',
  'Isolde', 'Lyris', 'Maren', 'Serafine', 'Veyra'
];

const femaleLastNames = [
  'Ashenveil', 'Bleakrose', 'Cindermere', 'Dawnveil', 'Emberly',
  'Gravenmoor', 'Hollowfen', 'Mirewood', 'Silverthorn', 'Wraithwood'
];

// 직업군별 시퀀스 카운터
const jobSequences: Record<JobClass, number> = {
  caster: 0,
  offenser: 0,
  defender: 0,
  attacker: 0,
  priest: 0,
  shaman: 0
};

/**
 * 랜덤 이름 생성
 */
export function generateAllyName(gender: Gender = 'male'): string {
  const firstNames = gender === 'male' ? maleFirstNames : femaleFirstNames;
  const lastNames = gender === 'male' ? maleLastNames : femaleLastNames;
  
  const firstName = firstNames[Math.floor(Math.random() * firstNames.length)];
  const lastName = lastNames[Math.floor(Math.random() * lastNames.length)];
  
  return `${firstName} ${lastName}`;
}

/**
 * 동료 ID 생성
 * 형식: ally_{jobClass}_{sequence}
 */
export function generateAllyId(jobClass: JobClass): string {
  jobSequences[jobClass]++;
  const sequence = jobSequences[jobClass].toString().padStart(2, '0');
  return `ally_${jobClass}_${sequence}`;
}

/**
 * 랜덤 성별 선택
 */
export function getRandomGender(): Gender {
  return Math.random() > 0.5 ? 'male' : 'female';
}

/**
 * 시퀀스 카운터 리셋 (테스트용)
 */
export function resetSequences() {
  Object.keys(jobSequences).forEach(key => {
    jobSequences[key as JobClass] = 0;
  });
}
