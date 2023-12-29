import { User } from './user';

export class UserParams {
  gender: string;
  minAge = 18;
  maxAge = 100;
  // pagination
  pageNumber = 1;
  pageSize = 10;
  // sorting
  orderBy = 'lastActive';

  constructor(user: User) {
    this.gender = user.gender === 'female' ? 'male' : 'female';
  }
}
