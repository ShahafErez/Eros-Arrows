import { User } from './user';

export class UserParams {
  desiredGender: string;
  minAge = 18;
  maxAge = 100;
  // pagination
  pageNumber = 1;
  pageSize = 10;
  // sorting
  orderBy = 'lastActive';

  constructor(user: User) {
    this.desiredGender = user.gender === 'female' ? 'male' : 'female';
  }
}
