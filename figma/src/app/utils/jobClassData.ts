/**
 * 직업 정보 데이터
 * 기획서: 04_캐릭터_시트_프로토타입.md 참조
 */

import { JobClass } from './allyNameGenerator';

export type AllyRole = 'dealer' | 'tank' | 'supporter';

export interface JobClassInfo {
  id: string;
  jobClass: JobClass;
  displayName: string; // UI 표시용 한글 이름
  role: AllyRole;
  maxHp: number;
  stressResist: number;
  recruitCost: number;
}

// MVP 6종 직업 정보
export const JOB_CLASS_DATA: Record<JobClass, JobClassInfo> = {
  caster: {
    id: 'ALLY_001',
    jobClass: 'caster',
    displayName: '캐스터',
    role: 'dealer',
    maxHp: 100,
    stressResist: 0,
    recruitCost: 30
  },
  offenser: {
    id: 'ALLY_002',
    jobClass: 'offenser',
    displayName: '오펜서',
    role: 'dealer',
    maxHp: 150,
    stressResist: 5,
    recruitCost: 30
  },
  defender: {
    id: 'ALLY_003',
    jobClass: 'defender',
    displayName: '디펜더',
    role: 'tank',
    maxHp: 200,
    stressResist: 10,
    recruitCost: 40
  },
  attacker: {
    id: 'ALLY_004',
    jobClass: 'attacker',
    displayName: '어택커',
    role: 'tank',
    maxHp: 150,
    stressResist: 10,
    recruitCost: 40
  },
  priest: {
    id: 'ALLY_005',
    jobClass: 'priest',
    displayName: '프리스트',
    role: 'supporter',
    maxHp: 100,
    stressResist: 0,
    recruitCost: 35
  },
  shaman: {
    id: 'ALLY_006',
    jobClass: 'shaman',
    displayName: '샤먼',
    role: 'supporter',
    maxHp: 100,
    stressResist: 10,
    recruitCost: 35
  }
};

/**
 * 역할에 따른 직업 목록 가져오기
 */
export function getJobClassesByRole(role: AllyRole): JobClass[] {
  return Object.values(JOB_CLASS_DATA)
    .filter(job => job.role === role)
    .map(job => job.jobClass);
}

/**
 * 랜덤 직업 선택
 */
export function getRandomJobClass(role?: AllyRole): JobClass {
  let jobClasses: JobClass[];
  
  if (role) {
    jobClasses = getJobClassesByRole(role);
  } else {
    jobClasses = Object.keys(JOB_CLASS_DATA) as JobClass[];
  }
  
  return jobClasses[Math.floor(Math.random() * jobClasses.length)];
}
